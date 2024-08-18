using GvatarWorkflow.Entities;

namespace GvatarWorkflow.Providers.Interfaces;

public interface IPersistenceProvider
{
    public Task<Guid> CreateNewWorkflowInstance(WorkflowDefinition workflowDefinition);

    public Task<Guid> CreateNewWorkflowInstance(WorkflowDefinition workflowDefinition, object? input);

    public Task<IEnumerable<WorkflowInstance>> GetAllWorkflowInstancesByIds(IEnumerable<Guid> ids);

    public Task<WorkflowInstance> GetWorkflowInstanceById(Guid workflowInstanceId);

    public Task PersistWorkflowInstance(WorkflowInstance workflowInstance);
}