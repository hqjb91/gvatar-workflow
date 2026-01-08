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

                bool stepCompleted = false;
                while (!stepCompleted)
                {
                    if (step.WaitFor is not null)
                    {
                        currentWorkflowInstance.Status = "Waiting";
                        currentWorkflowInstance.TaskCompletionSource = new TaskCompletionSource<bool>();
                        currentWorkflowInstance.EventTriggerName = step.WaitFor?.Item1;
                        activityInstance.Status = "Waiting";
                        activityInstance.WaitingForEvent = step.WaitFor?.Item1;
                        await _persistenceProvider.PersistWorkflowInstance(currentWorkflowInstance);
                        await currentWorkflowInstance.TaskCompletionSource.Task; // Waits for this task to complete before continuing
                        activityInstance.Status = "In Progress";
                        activityInstance.WaitingForEvent = null;
                        step.WaitFor?.Item2.Invoke(currentWorkflowInstance.CurrentStepObjectContext);
                        currentWorkflowInstance.Status = "In Progress";
                        await _persistenceProvider.PersistWorkflowInstance(currentWorkflowInstance);
                    }

                    try
                    {
                        var output = _delegateContext.InvokeDelegate(step.FunctionDelegateName, currentWorkflowInstance.CurrentStepObjectContext);
                        currentWorkflowInstance.CurrentStepObjectContext = output;
                        currentWorkflowInstance.PreviousCompletedStepIds.Add(step.Id);
                        currentWorkflowInstance.NextPendingStepIds.Remove(step.Id);
                        activityInstance.Output = output;
                        activityInstance.Status = "Completed";
                        activityInstance.CompletedAtUtc = DateTimeOffset.UtcNow;

                        MarkCompensationComplete(currentWorkflowInstance, step.Id);
                        EnqueueTransitions(step, currentWorkflowInstance, output, onFailure: false);
                        stepCompleted = true;
                    }
                    catch (Exception ex)
                    {
                        if (TryScheduleRetry(step, currentWorkflowInstance, activityInstance, ex))
                        {
                            await _persistenceProvider.PersistWorkflowInstance(currentWorkflowInstance);
                            continue;
                        }

                        activityInstance.Status = "Failed";
                        activityInstance.ErrorMessage = ex.Message;
                        activityInstance.CompletedAtUtc = DateTimeOffset.UtcNow;
                        currentWorkflowInstance.NextPendingStepIds.Remove(step.Id);

                        bool scheduledFailureTransitions = EnqueueTransitions(step, currentWorkflowInstance, currentWorkflowInstance.CurrentStepObjectContext, onFailure: true);
                        if (!scheduledFailureTransitions)
                        {
                            currentWorkflowInstance.Status = "Failed";
                            await _persistenceProvider.PersistWorkflowInstance(currentWorkflowInstance);
                            throw;
                        }

                        stepCompleted = true;
                    }

                    await _persistenceProvider.PersistWorkflowInstance(currentWorkflowInstance);
                }
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

    private static void MarkCompensationComplete(WorkflowInstance workflowInstance, Guid compensationStepId)
    {
        foreach (ActivityInstance activityInstance in workflowInstance.ActivityInstances
                     .Where(instance => instance.CompensationStepId == compensationStepId && !instance.CompensationExecuted))
        {
            activityInstance.CompensationExecuted = true;
            activityInstance.CompensationCompletedAtUtc = DateTimeOffset.UtcNow;
        }
    }

    private static IEnumerable<Transition> GetOrderedTransitions(Step step, bool onFailure)
    {
        return step.Transitions
            .Where(transition => transition.OnFailure == onFailure && transition.FromStepId == step.Id)
            .OrderBy(transition => transition.Order)
            .ThenBy(transition => transition.ToStepId);
    }

    private static bool EnqueueTransitions(Step step, WorkflowInstance workflowInstance, object? output, bool onFailure)
    {
        IEnumerable<Transition> transitions = GetOrderedTransitions(step, onFailure)
            .Where(transition => transition.Condition?.Evaluate(output) ?? true);
        bool hasTransitions = false;

        foreach (Transition transition in transitions)
        {
            hasTransitions = true;
            workflowInstance.NextPendingStepIds.Add(transition.ToStepId);
            if (transition.CompensationStepId is not null)
            {
                workflowInstance.NextPendingStepIds.Add(transition.CompensationStepId.Value);
                ActivityInstance? failedInstance = workflowInstance.ActivityInstances.LastOrDefault(instance => instance.StepId == step.Id);
                if (failedInstance is not null && failedInstance.CompensationStepId is null)
                {
                    failedInstance.CompensationStepId = transition.CompensationStepId;
                    failedInstance.CompensationAttempt = 1;
                    failedInstance.CompensationScheduledAtUtc = DateTimeOffset.UtcNow;
                }
            }
        }

        workflowInstance.NextPendingStepIds = workflowInstance.NextPendingStepIds.Distinct().ToList();
        return hasTransitions;
    }

    private static bool TryScheduleRetry(Step step, WorkflowInstance workflowInstance, ActivityInstance activityInstance, Exception exception)
    {
        Transition? retryTransition = GetOrderedTransitions(step, onFailure: true)
            .FirstOrDefault(transition => transition.RetryPolicy is not null
                                          && activityInstance.Attempt < transition.RetryPolicy.MaxAttempts
                                          && transition.ToStepId == step.Id
                                          && (transition.Condition?.Evaluate(workflowInstance.CurrentStepObjectContext) ?? true));

        if (retryTransition is null)
        {
            return false;
        }

        activityInstance.Attempt += 1;
        activityInstance.Status = "Retrying";
        activityInstance.ErrorMessage = exception.Message;
        workflowInstance.NextPendingStepIds.Add(retryTransition.ToStepId);
        workflowInstance.NextPendingStepIds = workflowInstance.NextPendingStepIds.Distinct().ToList();
        return true;
    }
}
