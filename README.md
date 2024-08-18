# Gvatar Workflow Engine

**Gvatar Workflow** is a simple, flexible workflow engine built as a .NET Class Library. It provides the capability to define and execute workflows with custom steps, persistence, and queue providers.

Currently the workflow supports only basic functionalities such as sequential flows and conditional branching.

It is a **work in progress** and there are plans to support suspended workflows pending events such as http requests.

## Features

- Define workflows with multiple steps. (Currently only supports sequential flows and conditional branching)
    - Workflows are defined via Fluent Builder - refer to the Sample Project or example below 
- Customizable persistence and queue providers. (Currently only supports and defaults to in-memory implementations)
- Extensible with custom delegates.
    - Delegates are classes that are associated with a particular step that contains the execution logic of the step.
- Sample project to demonstrate usage.

## Project Structure

- **GvatarWorkflow**: Core library of the workflow engine.
  - **Entities**: Key entities like `Step`, `WorkflowDefinition`, and `WorkflowInstance`.
  - **Providers**: Persistence and queue providers.
  - **Services**: Core services for workflow execution.
- **Sample1**: Sample ASP.NET Core project demonstrating how to use the workflow engine.

## Getting Started

### Prerequisites

- .NET 6.0 SDK or later

### Usage

#### Define a Workflow

Workflows can be defined by creating instances of `WorkflowDefinition` using the WorkflowDefinitionBuilder and adding `Step` instances to it. 
Each step can have a delegate that performs specific actions.

```csharp
private readonly IWorkflowDefinitionBuilder _workflowDefinitionBuilder;

Step step1 = new Step()
{
    Name = "Step1",
    FunctionDelegateName = "HelloWorldDelegate",
    ChildrenSteps = new List<string> { "Step2a", "Step2b" },
    Condition = null
};

Step step2a = new Step()
{
    Name = "Step2a",
    FunctionDelegateName = "MiddleDelegate",
    ChildrenSteps = new List<string> { "Step3" },
    Condition = null
};

Step step2b = new Step()
{
    Name = "Step2b",
    FunctionDelegateName = "MiddleDelegate2",
    ChildrenSteps = new List<string> { "Step3" },
    Condition = (input) => ((int)input > 1)
};

Step step3 = new Step()
{
    Name = "Step3",
    FunctionDelegateName = "EndWorkflowDelegate",
    ChildrenSteps = null,
    Condition = null
};

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
```

#### Executing a Workflow

Execute the workflow by simply starting the WorkflowService instance and starting a Workflow Instance with the associated Workflow Definition and Input.

```csharp
int testInput = 1;
_workflowService.StartWorkflowService();
_workflowService.StartWorkflowInstance(simpleWorkflowDefinition, testInput);
```

#### Creating Custom Delegates for the Steps

Workflows require Delegate classes to carry out the execution logic for each associated Step.
Delegate Classes inherit the IDelegate interface and are loaded via reflection based on Class names containing "Delegate". **(To be refactored to find the classes by interface instead)**

The Execute method defines the logic that will be ran during the step and the returned output will be passed on as an input to the next step.

```csharp

public class MiddleDelegate : IDelegate
{
    public object Execute(object input)
    {
        Console.WriteLine($"Middle step will multiply by 2");
        int intputAsNumber = (int)input;
        int output = intputAsNumber * 2;

        return output;
    }
}
```

## License

This project is licensed under the MIT License.