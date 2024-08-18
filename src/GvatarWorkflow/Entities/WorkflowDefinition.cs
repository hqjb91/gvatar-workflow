using GvatarWorkflow.Entities.Interfaces;

namespace GvatarWorkflow.Entities;

public class WorkflowDefinition
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int Version { get; set; }
    public List<Step> Steps { get; set; } = [];
}

public class WorkflowDefinitionBuilder : IWorkflowDefinitionBuilder
{
    private readonly WorkflowDefinition _workflowDefinition = new();

    public IWorkflowDefinitionBuilder AddDescription(string description)
    {
        _workflowDefinition.Description = description;
        return this;
    }

    public IWorkflowDefinitionBuilder AddName(string name)
    {
        _workflowDefinition.Name = name;
        return this;
    }

    public IWorkflowDefinitionBuilder AddStep(Step step)
    {
        _workflowDefinition.Steps.Add(step);
        return this;
    }

    public IWorkflowDefinitionBuilder AddVersion(int version)
    {
        _workflowDefinition.Version = version;
        return this;
    }

    public WorkflowDefinition Build()
    {
        Guid guid = new();
        _workflowDefinition.Id = guid;
        return _workflowDefinition;
    }
}
