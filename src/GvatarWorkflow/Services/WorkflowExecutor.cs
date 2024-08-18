using GvatarWorkflow.Context;
using GvatarWorkflow.Entities;
using GvatarWorkflow.Providers.Interfaces;
using GvatarWorkflow.Services.Interfaces;
using System.Diagnostics;

namespace GvatarWorkflow.Services;

public class WorkflowExecutor(IPersistenceProvider persistenceProvider, DelegateContext delegateContext) : IWorkflowExecutor
{
    private readonly IPersistenceProvider _persistenceProvider = persistenceProvider;
    private readonly DelegateContext _delegateContext = delegateContext;

    public async Task ExecuteWorkflowInstance(Guid workflowInstanceId)
    {
        WorkflowInstance currentWorkflowInstance = await _persistenceProvider.GetWorkflowInstanceById(workflowInstanceId);
        currentWorkflowInstance.Status = "In Progress";

        while (currentWorkflowInstance.NextPendingStepNames.Count > 0)
        {
            List<Step> currentStepsToExecute = currentWorkflowInstance.WorkflowDefinition.Steps.Where(step => currentWorkflowInstance.NextPendingStepNames.Contains(step.Name)).ToList();

            foreach(Step step in currentStepsToExecute)
            {
                var output = _delegateContext.InvokeDelegate(step.FunctionDelegateName, currentWorkflowInstance.CurrentStepObjectContext, step.Condition);
                currentWorkflowInstance.CurrentStepObjectContext = output;
                currentWorkflowInstance.PreviousCompletedStepNames.Add(step.Name);
                currentWorkflowInstance.NextPendingStepNames.Remove(step.Name);
                if (step.ChildrenSteps is not null)
                {
                    currentWorkflowInstance.NextPendingStepNames.AddRange(step.ChildrenSteps);
                    currentWorkflowInstance.NextPendingStepNames = currentWorkflowInstance.NextPendingStepNames.Distinct().ToList();
                }
            }
        }

        currentWorkflowInstance.Status = "Completed";
    }
}