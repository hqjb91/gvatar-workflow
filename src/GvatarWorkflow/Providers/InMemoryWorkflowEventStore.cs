using GvatarWorkflow.Entities;
using GvatarWorkflow.Providers.Interfaces;

namespace GvatarWorkflow.Providers;

public sealed class InMemoryWorkflowEventStore(SingletonInMemoryWorkflowEventStore singletonInMemoryWorkflowEventStore) : IWorkflowEventStore
{
    private readonly SingletonInMemoryWorkflowEventStore _singletonInMemoryWorkflowEventStore = singletonInMemoryWorkflowEventStore;

    public Task<WorkflowEventWait?> GetWaitForStep(Guid workflowInstanceId, Guid stepId)
    {
        return _singletonInMemoryWorkflowEventStore.GetWaitForStep(workflowInstanceId, stepId);
    }

    public Task<WorkflowEventWait> CreateWait(WorkflowEventWait wait)
    {
        return _singletonInMemoryWorkflowEventStore.CreateWait(wait);
    }

    public Task<IReadOnlyList<WorkflowEventWait>> GetWaitsByEvent(string eventName, IReadOnlyCollection<WorkflowEventKey> correlationKeys)
    {
        return _singletonInMemoryWorkflowEventStore.GetWaitsByEvent(eventName, correlationKeys);
    }

    public Task RemoveWait(Guid waitId)
    {
        return _singletonInMemoryWorkflowEventStore.RemoveWait(waitId);
    }

    public Task<WorkflowEventRecord> RecordEvent(WorkflowEventRecord workflowEvent)
    {
        return _singletonInMemoryWorkflowEventStore.RecordEvent(workflowEvent);
    }

    public Task<WorkflowEventRecord?> FindMatchingEvent(string eventName, IReadOnlyCollection<WorkflowEventKey> correlationKeys)
    {
        return _singletonInMemoryWorkflowEventStore.FindMatchingEvent(eventName, correlationKeys);
    }

    public Task RemoveEvent(Guid eventId)
    {
        return _singletonInMemoryWorkflowEventStore.RemoveEvent(eventId);
    }
}
