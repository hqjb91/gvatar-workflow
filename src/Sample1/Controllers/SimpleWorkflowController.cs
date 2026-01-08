using GvatarWorkflow.Entities;
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
            FunctionDelegateName = "HelloWorldDelegate"
        };

        Step step2a = new()
        {
            Name = "Step2a",
            FunctionDelegateName = "MiddleDelegate"
        };

        Step step2b = new()
        {
            Name = "Step2b",
            FunctionDelegateName = "MiddleDelegate2"
        };

        Step step3 = new()
        {
            Name = "Step3",
            FunctionDelegateName = "EndWorkflowDelegate",
            Transitions = []
        };

        step1.Transitions =
        [
            new Transition
            {
                FromStepId = step1.Id,
                ToStepId = step2a.Id,
                Condition = new TransitionCondition
                {
                    Type = TransitionConditionType.LessThanOrEqual,
                    Value = "1"
                },
                Order = 1
            },
            new Transition
            {
                FromStepId = step1.Id,
                ToStepId = step2b.Id,
                Condition = new TransitionCondition
                {
                    Type = TransitionConditionType.GreaterThan,
                    Value = "1"
                },
                Order = 2
            }
        ];
        step2a.Transitions =
        [
            new Transition
            {
                FromStepId = step2a.Id,
                ToStepId = step3.Id,
                Condition = new TransitionCondition { Type = TransitionConditionType.Always },
                Order = 1
            }
        ];
        step2b.Transitions =
        [
            new Transition
            {
                FromStepId = step2b.Id,
                ToStepId = step3.Id,
                Condition = new TransitionCondition { Type = TransitionConditionType.Always },
                Order = 1
            }
        ];

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
