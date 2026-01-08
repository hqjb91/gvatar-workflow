namespace GvatarWorkflow.Entities;
public class WorkflowInstance(
    string status,
    List<string> previousCompletedStepNames,
    List<string> nextPendingStepNames,
    object? currentStepObjectContext,
    WorkflowDefinition workflowDefinition,
    List<ActivityInstance>? activityInstances = null
    )
{
    public Guid Id { get; set; }
    public string Status { get; set; } = status;
    public List<string> PreviousCompletedStepNames { get; set; } = previousCompletedStepNames;
    public List<string> NextPendingStepNames { get; set; } = nextPendingStepNames;
    public object? CurrentStepObjectContext { get; set; } = currentStepObjectContext;
    public WorkflowDefinition WorkflowDefinition { get; set; } = workflowDefinition;
    public List<ActivityInstance> ActivityInstances { get; set; } = activityInstances ?? [];
    public TaskCompletionSource<bool>? TaskCompletionSource { get; set; } = null!;
    public string? EventTriggerName { get; set; } = "";
}
