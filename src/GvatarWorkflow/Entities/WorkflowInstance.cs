namespace GvatarWorkflow.Entities;
public class WorkflowInstance(
    string status,
    List<string> previousCompletedStepNames,
    List<string> nextPendingStepNames,
    object? currentStepObjectContext,
    WorkflowDefinition workflowDefinition
    )
{
    public Guid Id { get; set; }
    public string Status { get; set; } = status;
    public List<string> PreviousCompletedStepNames { get; set; } = previousCompletedStepNames;
    public List<string> NextPendingStepNames { get; set; } = nextPendingStepNames;
    public object? CurrentStepObjectContext { get; set; } = currentStepObjectContext;
    public WorkflowDefinition WorkflowDefinition { get; set; } = workflowDefinition;
}
