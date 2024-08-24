using GvatarWorkflow.Entities;

namespace GvatarWorkflow.Services.Interfaces;

public interface IWorkflowExecutor
{
    public Task ExecuteWorkflowInstance(Guid workflowInstanceId);
    public Task ContinueWorkflowInstance(WorkflowInstance workflowInstance, string eventTriggerName);
}