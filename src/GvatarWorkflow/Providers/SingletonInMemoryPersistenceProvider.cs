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
        lock(_instances)
        {
            return Task.Run(() =>
            {
                Guid newGuid = Guid.NewGuid();
                WorkflowInstance newWorkflowInstance = new("New", [], [workflowDefinition.Steps[0].Name], null, workflowDefinition)
                {
                    Id = newGuid,
                    CurrentStepObjectContext = input
                };
                _instances.Add(newWorkflowInstance);
                return newGuid;
            });
        }
    }

    public Task<IEnumerable<WorkflowInstance>> GetAllWorkflowInstancesByIds(IEnumerable<Guid> ids)
    {
        lock (_instances)
        {
            return Task.Run(() =>
            {
                return _instances.Where(instance => ids.Contains(instance.Id));
            });
        }
    }

    public Task<WorkflowInstance> GetWorkflowInstanceById(Guid workflowInstanceId)
    {
        lock (_instances) 
        {
            return Task.Run(() =>
            {
                return _instances.Where(instance => instance.Id == workflowInstanceId).First();
            });
        }
    }

    public Task PersistWorkflowInstance(WorkflowInstance workflowInstance)
    {
        lock(_instances)
        {
            return Task.Run(() =>
            {
                var existing = _instances.First(instance => instance.Id == workflowInstance.Id);
                _instances.Remove(existing);
                _instances.Add(workflowInstance);
            });
        }
    }
}