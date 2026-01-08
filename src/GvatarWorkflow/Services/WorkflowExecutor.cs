using GvatarWorkflow.Context;
using GvatarWorkflow.Entities;
using GvatarWorkflow.Providers.Interfaces;
using GvatarWorkflow.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace GvatarWorkflow.Services;

public class WorkflowExecutor(IPersistenceProvider persistenceProvider, IWorkflowEventStore workflowEventStore, DelegateContext delegateContext, IServiceProvider serviceProvider) : IWorkflowExecutor
{
    private readonly IPersistenceProvider _persistenceProvider = persistenceProvider;
    private readonly IWorkflowEventStore _workflowEventStore = workflowEventStore;
    private readonly DelegateContext _delegateContext = delegateContext;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task ExecuteWorkflowInstance(Guid workflowInstanceId)
    {
        WorkflowInstance currentWorkflowInstance = await _persistenceProvider.GetWorkflowInstanceById(workflowInstanceId);
        currentWorkflowInstance.Status = "In Progress";
        await _persistenceProvider.PersistWorkflowInstance(currentWorkflowInstance);

        SemaphoreSlim updateLock = new(1, 1);

        while (currentWorkflowInstance.NextPendingStepIds.Count > 0)
        {
            List<Step> currentStepsToExecute = currentWorkflowInstance.WorkflowDefinition.Steps
                .Where(step => currentWorkflowInstance.NextPendingStepIds.Contains(step.Id))
                .ToList();

            object? inputSnapshot = currentWorkflowInstance.CurrentStepObjectContext;

            foreach (Step step in currentStepsToExecute)
            {
                await updateLock.WaitAsync();
                ActivityInstance? activityInstance;
                try
                {
                    activityInstance = currentWorkflowInstance.ActivityInstances
                        .FirstOrDefault(instance => instance.StepId == step.Id && instance.Status == "Waiting");

                    if (activityInstance is null)
                    {
                        activityInstance = new ActivityInstance(step.Id, step.Name)
                        {
                            Status = "In Progress",
                            StartedAtUtc = DateTimeOffset.UtcNow,
                            Attempt = 1
                        };
                        currentWorkflowInstance.ActivityInstances.Add(activityInstance);
                        await _persistenceProvider.PersistWorkflowInstance(currentWorkflowInstance);
                    }
                }
                finally
                {
                    updateLock.Release();
                }

                if (activityInstance is null)
                {
                    throw new InvalidOperationException($"Activity instance for step '{step.Name}' was not created.");
                }

                if (step.WaitFor is not null)
                {
                    string eventName = step.WaitFor.Value.Item1;
                    IReadOnlyCollection<WorkflowEventKey> correlationKeys = BuildCorrelationKeys(currentWorkflowInstance);

                    WorkflowEventWait? existingWait = await _workflowEventStore.GetWaitForStep(currentWorkflowInstance.Id, step.Id);
                    WorkflowEventWait wait = existingWait ?? new WorkflowEventWait
                    {
                        WorkflowInstanceId = currentWorkflowInstance.Id,
                        StepId = step.Id,
                        StepName = step.Name,
                        EventName = eventName,
                        CorrelationKeys = correlationKeys.ToList()
                    };

                    if (existingWait is null)
                    {
                        await _workflowEventStore.CreateWait(wait);
                    }

                    WorkflowEventRecord? matchingEvent = await _workflowEventStore.FindMatchingEvent(eventName, wait.CorrelationKeys);
                    if (matchingEvent is null)
                    {
                        await updateLock.WaitAsync();
                        try
                        {
                            currentWorkflowInstance.Status = "Waiting";
                            activityInstance.Status = "Waiting";
                            activityInstance.WaitingForEvent = eventName;
                            await _persistenceProvider.PersistWorkflowInstance(currentWorkflowInstance);
                        }
                        finally
                        {
                            updateLock.Release();
                        }
                        return;
                    }

                    await _workflowEventStore.RemoveEvent(matchingEvent.Id);
                    await _workflowEventStore.RemoveWait(wait.Id);
                    await updateLock.WaitAsync();
                    try
                    {
                        activityInstance.Status = "In Progress";
                        activityInstance.WaitingForEvent = null;
                        step.WaitFor.Value.Item2.Invoke(currentWorkflowInstance.CurrentStepObjectContext);
                        currentWorkflowInstance.Status = "In Progress";
                        await _persistenceProvider.PersistWorkflowInstance(currentWorkflowInstance);
                    }
                    finally
                    {
                        updateLock.Release();
                    }
                }
            }

            Task<object?>[] stepTasks = currentStepsToExecute
                .Select(step => ExecuteStepAsync(step, inputSnapshot, currentWorkflowInstance, updateLock))
                .ToArray();

            object?[] outputs = await Task.WhenAll(stepTasks);

            await updateLock.WaitAsync();
            try
            {
                currentWorkflowInstance.CurrentStepObjectContext = outputs.Length == 1 ? outputs[0] : outputs.ToList();
                await _persistenceProvider.PersistWorkflowInstance(currentWorkflowInstance);
            }
            finally
            {
                updateLock.Release();
            }
        }

        currentWorkflowInstance.Status = "Completed";
        await _persistenceProvider.PersistWorkflowInstance(currentWorkflowInstance);
    }

    public async Task ContinueWorkflowInstance(string eventTriggerName, IReadOnlyCollection<WorkflowEventKey> correlationKeys)
    {
        WorkflowEventRecord workflowEvent = new()
        {
            EventName = eventTriggerName,
            CorrelationKeys = correlationKeys.ToList()
        };

        await _workflowEventStore.RecordEvent(workflowEvent);

        IReadOnlyList<WorkflowEventWait> waits = await _workflowEventStore.GetWaitsByEvent(eventTriggerName, correlationKeys);
        IQueueProvider queueProvider = _serviceProvider.GetRequiredService<IQueueProvider>();
        foreach (WorkflowEventWait wait in waits)
        {
            await queueProvider.QueueWork(wait.WorkflowInstanceId, QueueType.Workflow);
        }
    }

    private static IReadOnlyCollection<WorkflowEventKey> BuildCorrelationKeys(WorkflowInstance workflowInstance)
    {
        return [new WorkflowEventKey("workflowInstanceId", workflowInstance.Id.ToString())];
    }

    private async Task<object?> ExecuteStepAsync(
        Step step,
        object? inputSnapshot,
        WorkflowInstance currentWorkflowInstance,
        SemaphoreSlim updateLock)
    {
        ActivityInstance activityInstance = currentWorkflowInstance.ActivityInstances
            .First(instance => instance.StepId == step.Id && instance.Status != "Waiting");

        object? output;

        try
        {
            output = _delegateContext.InvokeDelegate(step.FunctionDelegateName, inputSnapshot, step.Condition);
        }
        catch (Exception ex)
        {
            await updateLock.WaitAsync();
            try
            {
                activityInstance.Status = "Failed";
                activityInstance.ErrorMessage = ex.Message;
                activityInstance.CompletedAtUtc = DateTimeOffset.UtcNow;
                currentWorkflowInstance.Status = "Failed";
                await _persistenceProvider.PersistWorkflowInstance(currentWorkflowInstance);
            }
            finally
            {
                updateLock.Release();
            }
            throw;
        }

        await updateLock.WaitAsync();
        try
        {
            currentWorkflowInstance.PreviousCompletedStepIds.Add(step.Id);
            currentWorkflowInstance.NextPendingStepIds.Remove(step.Id);
            activityInstance.Output = output;
            activityInstance.Status = "Completed";
            activityInstance.CompletedAtUtc = DateTimeOffset.UtcNow;

            if (step.ChildrenSteps is not null)
            {
                currentWorkflowInstance.NextPendingStepIds.AddRange(step.ChildrenSteps);
                currentWorkflowInstance.NextPendingStepIds = currentWorkflowInstance.NextPendingStepIds.Distinct().ToList();
            }

            await _persistenceProvider.PersistWorkflowInstance(currentWorkflowInstance);
        }
        finally
        {
            updateLock.Release();
        }
        return output;
    }
}
