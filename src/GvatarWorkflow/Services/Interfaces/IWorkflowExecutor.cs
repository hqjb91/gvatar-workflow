namespace GvatarWorkflow.Services.Interfaces;

public interface IWorkflowExecutor
{
    public Task ExecuteWorkflowInstance(Guid workflowInstanceId);
}