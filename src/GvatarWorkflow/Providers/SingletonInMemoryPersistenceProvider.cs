using GvatarWorkflow.Entities;
using GvatarWorkflow.Providers.Interfaces;

namespace GvatarWorkflow.Providers;
public class SingletonInMemoryPersistenceProvider : IPersistenceProvider
{
    private readonly List<WorkflowInstance> _instances = [];

    public async Task<Guid> CreateNewWorkflowInstance(WorkflowDefinition workflowDefinition)
    {
        return await CreateNewWorkflowInstance(workflowDefinition, null);
    }
    public Task<Guid> CreateNewWorkflowInstance(WorkflowDefinition workflowDefinition, object? input)
    {
        lock (_instances)
        {
            Guid newGuid = Guid.NewGuid();
            WorkflowInstance newWorkflowInstance = new("New", [], [workflowDefinition.Steps[0].Id], null, workflowDefinition)
            {
                Id = newGuid,
                CurrentStepObjectContext = input
            };
            _instances.Add(newWorkflowInstance);
            return newGuid;
        }
    }

    public Task<IEnumerable<WorkflowInstance>> GetAllWorkflowInstancesByIds(IEnumerable<Guid> ids)
    {
        lock (_instances)
        {
            return Task.FromResult<IEnumerable<WorkflowInstance>>(_instances.Where(instance => ids.Contains(instance.Id)).ToList());
        }
    }

    public Task<WorkflowInstance> GetWorkflowInstanceById(Guid workflowInstanceId)
    {
        lock (_instances) 
        {
            WorkflowInstance? instance = _instances.FirstOrDefault(item => item.Id == workflowInstanceId);
            if (instance is null)
            {
                throw new KeyNotFoundException($"No workflow instance found for id {workflowInstanceId}.");
            }

            return Task.FromResult(instance);
        }
    }

    public Task PersistWorkflowInstance(WorkflowInstance workflowInstance)
    {
        lock(_instances)
        {
            var existing = _instances.FirstOrDefault(instance => instance.Id == workflowInstance.Id);
            if (existing is null)
            {
                throw new KeyNotFoundException($"No workflow instance found for id {workflowInstance.Id}.");
            }

            _instances.Remove(existing);
            _instances.Add(workflowInstance);
            return Task.CompletedTask;
        }
    }
}
