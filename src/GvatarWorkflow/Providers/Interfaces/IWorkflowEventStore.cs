using GvatarWorkflow.Entities;

namespace GvatarWorkflow.Providers.Interfaces;

public interface IWorkflowEventStore
{
    public Task<WorkflowEventWait?> GetWaitForStep(Guid workflowInstanceId, Guid stepId);
    public Task<WorkflowEventWait> CreateWait(WorkflowEventWait wait);
    public Task<IReadOnlyList<WorkflowEventWait>> GetWaitsByEvent(string eventName, IReadOnlyCollection<WorkflowEventKey> correlationKeys);
    public Task RemoveWait(Guid waitId);
    public Task<WorkflowEventRecord> RecordEvent(WorkflowEventRecord workflowEvent);
    public Task<WorkflowEventRecord?> FindMatchingEvent(string eventName, IReadOnlyCollection<WorkflowEventKey> correlationKeys);
    public Task RemoveEvent(Guid eventId);
}
