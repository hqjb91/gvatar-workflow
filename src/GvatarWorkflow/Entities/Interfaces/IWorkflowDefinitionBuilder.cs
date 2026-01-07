namespace GvatarWorkflow.Entities.Interfaces;

public interface IWorkflowDefinitionBuilder
{
    public IWorkflowDefinitionBuilder AddStep(Step step);
    public IWorkflowDefinitionBuilder AddDescription(string description);
    public IWorkflowDefinitionBuilder AddVersion(int version);
    public IWorkflowDefinitionBuilder AddName(string name);
    public IWorkflowDefinitionBuilder AddId(Guid id);
    public WorkflowDefinition Build();
}
