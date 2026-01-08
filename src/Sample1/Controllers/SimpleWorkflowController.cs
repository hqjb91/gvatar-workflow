using GvatarWorkflow.Entities;
using GvatarWorkflow.Entities.Interfaces;
using GvatarWorkflow.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Sample1.Controllers;

[ApiController]
[Route("[controller]")]
public class SimpleWorkflowController(ILogger<SimpleWorkflowController> logger, IWorkflowService workflowService, IWorkflowDefinitionBuilder workflowDefinitionBuilder) : ControllerBase
{
    private readonly ILogger<SimpleWorkflowController> _logger = logger;
    private readonly IWorkflowService _workflowService = workflowService;
    private readonly IWorkflowDefinitionBuilder _workflowDefinitionBuilder = workflowDefinitionBuilder;

    [HttpGet(Name = "ExecuteSimpleWorkflow")]
    public async Task Get()
    {
        Console.WriteLine("Starting Simple Workflow...");

        Step step1 = new()
        {
            Name = "Step1",
            FunctionDelegateName = "HelloWorldDelegate",
            Condition = null
        };

        Step step2a = new()
        {
            Name = "Step2a",
            FunctionDelegateName = "MiddleDelegate",
            Condition = null
        };

        Step step2b = new()
        {
            Name = "Step2b",
            FunctionDelegateName = "MiddleDelegate2",
            Condition = (input) => ((int?)input > 1)
        };

        Step step3 = new()
        {
            Name = "Step3",
            FunctionDelegateName = "EndWorkflowDelegate",
            ChildrenSteps = null,
            Condition = null
        };

        step1.ChildrenSteps = [step2a.Id, step2b.Id];
        step2a.ChildrenSteps = [step3.Id];
        step2b.ChildrenSteps = [step3.Id];

        WorkflowDefinition simpleWorkflowDefinition =
            _workflowDefinitionBuilder
                .AddName("Simple Workflow")
                .AddDescription("Simple Workflow")
                .AddVersion(1)
                .AddStep(step1)
                .AddStep(step2a)
                .AddStep(step2b)
                .AddStep(step3)
                .Build();

        int testInput = 1;
        await _workflowService.StartWorkflowService();
        await _workflowService.StartWorkflowInstance(simpleWorkflowDefinition, testInput);
    }
}
