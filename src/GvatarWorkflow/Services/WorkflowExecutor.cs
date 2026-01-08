using GvatarWorkflow.Context;
using GvatarWorkflow.Entities;
using GvatarWorkflow.Providers.Interfaces;
using GvatarWorkflow.Services.Interfaces;

namespace GvatarWorkflow.Services;

public class WorkflowExecutor(IPersistenceProvider persistenceProvider, DelegateContext delegateContext) : IWorkflowExecutor
{
    private readonly IPersistenceProvider _persistenceProvider = persistenceProvider;
    private readonly DelegateContext _delegateContext = delegateContext;

    public async Task ExecuteWorkflowInstance(Guid workflowInstanceId)
    {
        WorkflowInstance currentWorkflowInstance = await _persistenceProvider.GetWorkflowInstanceById(workflowInstanceId);
        currentWorkflowInstance.Status = "In Progress";
        await _persistenceProvider.PersistWorkflowInstance(currentWorkflowInstance);

        while (currentWorkflowInstance.NextPendingStepIds.Count > 0)
        {
            List<Step> currentStepsToExecute = currentWorkflowInstance.WorkflowDefinition.Steps
                .Where(step => currentWorkflowInstance.NextPendingStepIds.Contains(step.Id))
                .ToList();

            foreach (Step step in currentStepsToExecute)
            {
                ActivityInstance activityInstance = new(step.Id, step.Name)
                {
                    Status = "In Progress",
                    StartedAtUtc = DateTimeOffset.UtcNow,
                    Attempt = 1
                };
                currentWorkflowInstance.ActivityInstances.Add(activityInstance);
                await _persistenceProvider.PersistWorkflowInstance(currentWorkflowInstance);

                if (step.WaitFor is not null)
                {
                    currentWorkflowInstance.TaskCompletionSource = new TaskCompletionSource<bool>();
                    currentWorkflowInstance.EventTriggerName = step.WaitFor?.Item1;
                    activityInstance.Status = "Waiting";
                    activityInstance.WaitingForEvent = step.WaitFor?.Item1;
                    await _persistenceProvider.PersistWorkflowInstance(currentWorkflowInstance);
                    await currentWorkflowInstance.TaskCompletionSource.Task; // Waits for this task to complete before continuing
                    activityInstance.Status = "In Progress";
                    activityInstance.WaitingForEvent = null;
                    step.WaitFor?.Item2.Invoke(currentWorkflowInstance.CurrentStepObjectContext);
                }

                try
                {
                    var output = _delegateContext.InvokeDelegate(step.FunctionDelegateName, currentWorkflowInstance.CurrentStepObjectContext, step.Condition);
                    currentWorkflowInstance.CurrentStepObjectContext = output;
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
                }
                catch (Exception ex)
                {
                    activityInstance.Status = "Failed";
                    activityInstance.ErrorMessage = ex.Message;
                    activityInstance.CompletedAtUtc = DateTimeOffset.UtcNow;
                    currentWorkflowInstance.Status = "Failed";
                    await _persistenceProvider.PersistWorkflowInstance(currentWorkflowInstance);
                    throw;
                }

                await _persistenceProvider.PersistWorkflowInstance(currentWorkflowInstance);
            }
        }

        currentWorkflowInstance.Status = "Completed";
        await _persistenceProvider.PersistWorkflowInstance(currentWorkflowInstance);
    }

    public Task ContinueWorkflowInstance(WorkflowInstance workflowInstance, string eventTriggerName)
    {
        Task.Run(() =>
        {
            if (workflowInstance.EventTriggerName == eventTriggerName)
            {
                Console.WriteLine($"Continuing workflow for event: {eventTriggerName}");
                workflowInstance.TaskCompletionSource?.TrySetResult(true);
            }
            else
            {
                Console.WriteLine($"No matching event found for: {eventTriggerName}. Expecting {workflowInstance.EventTriggerName}");
            }
        });

        return Task.CompletedTask;
    }
}
