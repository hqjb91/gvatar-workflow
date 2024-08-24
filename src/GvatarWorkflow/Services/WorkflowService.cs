using GvatarWorkflow.Entities;
using GvatarWorkflow.Providers.Interfaces;
using GvatarWorkflow.Services.Interfaces;

namespace GvatarWorkflow.Services;

public class WorkflowService(IPersistenceProvider persistenceProvider, IQueueProvider queueProvider) : IWorkflowService
{
    private readonly IPersistenceProvider _persistenceProvider = persistenceProvider;
    private readonly IQueueProvider _queueProvider = queueProvider;

    public async Task StartWorkflowInstance(WorkflowDefinition workflowDefinition)
    {
        await StartWorkflowInstance(workflowDefinition, null);
    }

    public async Task<Guid> StartWorkflowInstance(WorkflowDefinition workflowDefinition, object? input)
    {
        Guid createdWorkflowId = await _persistenceProvider.CreateNewWorkflowInstance(workflowDefinition, input);
        await QueueWorkflowInstance(createdWorkflowId);

        return createdWorkflowId;
    }

    public async Task<WorkflowInstance> GetWorkflowInstanceById(Guid workflowInstanceId)
    {
        return await _persistenceProvider.GetWorkflowInstanceById(workflowInstanceId);
    }

    public async Task QueueWorkflowInstance(Guid workflowInstanceId)
    {
        await _queueProvider.QueueWork(workflowInstanceId, QueueType.Workflow);
    }

    public async Task StartWorkflowService()
    {
        await _queueProvider.Start();
    }
}
