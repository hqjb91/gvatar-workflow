namespace GvatarWorkflow.Entities;
public class WorkflowInstance(
    string status,
    List<Guid> previousCompletedStepIds,
    List<Guid> nextPendingStepIds,
    object? currentStepObjectContext,
    WorkflowDefinition workflowDefinition,
    List<ActivityInstance>? activityInstances = null
    )
{
    public Guid Id { get; set; }
    public string Status { get; set; } = status;
    public List<Guid> PreviousCompletedStepIds { get; set; } = previousCompletedStepIds;
    public List<Guid> NextPendingStepIds { get; set; } = nextPendingStepIds;
    public object? CurrentStepObjectContext { get; set; } = currentStepObjectContext;
    public WorkflowDefinition WorkflowDefinition { get; set; } = workflowDefinition;
    public List<ActivityInstance> ActivityInstances { get; set; } = activityInstances ?? [];
}
