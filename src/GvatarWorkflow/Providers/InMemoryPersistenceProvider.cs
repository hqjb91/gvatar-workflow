using GvatarWorkflow.Entities;
using GvatarWorkflow.Providers.Interfaces;

namespace GvatarWorkflow.Providers;

public class InMemoryPersistenceProvider(SingletonInMemoryPersistenceProvider singletonInMemoryPersistenceProvider) : IPersistenceProvider
{
    private readonly SingletonInMemoryPersistenceProvider _singletonInMemoryPersistenceProvider = singletonInMemoryPersistenceProvider;

    public async Task<Guid> CreateNewWorkflowInstance(WorkflowDefinition workflowDefinition)
    {
        return await _singletonInMemoryPersistenceProvider.CreateNewWorkflowInstance(workflowDefinition);
    }

    public async Task<Guid> CreateNewWorkflowInstance(WorkflowDefinition workflowDefinition, object? input)
    {
        return await _singletonInMemoryPersistenceProvider.CreateNewWorkflowInstance(workflowDefinition, input);
    }

    public async Task<IEnumerable<WorkflowInstance>> GetAllWorkflowInstancesByIds(IEnumerable<Guid> ids)
    {
        return await _singletonInMemoryPersistenceProvider.GetAllWorkflowInstancesByIds(ids);
    }

    public async Task<WorkflowInstance> GetWorkflowInstanceById(Guid workflowInstanceId)
    {
        return await _singletonInMemoryPersistenceProvider.GetWorkflowInstanceById(workflowInstanceId);
    }

    public async Task PersistWorkflowInstance(WorkflowInstance workflowInstance)
    {
        await _singletonInMemoryPersistenceProvider.PersistWorkflowInstance(workflowInstance);
    }
}
