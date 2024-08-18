using GvatarWorkflow.Entities;

namespace GvatarWorkflow.Services.Interfaces;

public interface IWorkflowService
{
    public Task StartWorkflowInstance(WorkflowDefinition workflowDefinition);
    public Task StartWorkflowInstance(WorkflowDefinition workflowDefinition, object input);
    public Task<WorkflowInstance> GetWorkflowInstanceById(Guid workflowInstanceId);
    public Task QueueWorkflowInstance(Guid workflowInstanceId);
    public Task StartWorkflowService();
}
