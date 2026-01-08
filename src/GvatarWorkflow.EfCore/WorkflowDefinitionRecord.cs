using GvatarWorkflow.Entities;

namespace GvatarWorkflow.Providers;

public sealed class WorkflowDefinitionRecord
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int Version { get; set; }
    public List<StepRecord> Steps { get; set; } = [];

    public static WorkflowDefinitionRecord FromDefinition(WorkflowDefinition workflowDefinition)
    {
        return new WorkflowDefinitionRecord
        {
            Id = workflowDefinition.Id,
            Name = workflowDefinition.Name,
            Description = workflowDefinition.Description,
            Version = workflowDefinition.Version,
            Steps = workflowDefinition.Steps.Select(StepRecord.FromStep).ToList()
        };
    }

    public WorkflowDefinition ToDefinition()
    {
        return new WorkflowDefinition
        {
            Id = Id,
            Name = Name,
            Description = Description,
            Version = Version,
            Steps = Steps.Select(stepRecord => stepRecord.ToStep()).ToList()
        };
    }
}

public sealed class StepRecord
{
    public string Name { get; set; } = "";
    public string FunctionDelegateName { get; set; } = "";
    public List<string>? ChildrenSteps { get; set; }

    public static StepRecord FromStep(Step step)
    {
        return new StepRecord
        {
            Name = step.Name,
            FunctionDelegateName = step.FunctionDelegateName,
            ChildrenSteps = step.ChildrenSteps?.ToList()
        };
    }

    public Step ToStep()
    {
        return new Step
        {
            Name = Name,
            FunctionDelegateName = FunctionDelegateName,
            ChildrenSteps = ChildrenSteps
        };
    }
}
