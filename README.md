# Gvatar Workflow Engine

**Gvatar Workflow** is a simple, flexible workflow engine built as a .NET Class Library. It provides the capability to define and execute workflows with custom steps, persistence, and queue providers.

Currently the workflow supports functionalities such as sequential flows, conditional branching and tasks pending events.

It is a **work in progress**.

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
Delegate Classes inherit the IDelegate interface and are loaded via reflection automatically.

The Execute method defines the logic that will be ran during the step and the returned output will be passed on as an input to the next step.

#### 

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

#### Specifying a multiple children steps workflow

When defining a step we can specify a list of steps as the children step.
These steps will be ran in parallel.

```csharp
Step step1 = new()
{
    Name = "Step1",
    FunctionDelegateName = "HelloWorldDelegate",
    ChildrenSteps = ["Step2a", "Step2b"],
    Condition = null,
    WaitFor = null
};
```

#### Specifying a Condition to continue the workflow

When defining a step we can specify a Function delegate that takes in an input object and returns a boolean.
The boolean output determines whether the workflow is terminated or continues.

```csharp
Step step2b = new()
{
    Name = "Step2b",
    FunctionDelegateName = "MiddleDelegate2",
    ChildrenSteps = ["Step3"],
    Condition = (input) => ((int?)input > 1),
    WaitFor = null
};
```

#### Suspending the workflow pending an event trigger (Refer to Sample 2 for example of implementation)

When defining a step we can specify a tuple with the first input being the name of the continue workflow event trigger and the second
input being a Function delegate will be ran before the continuation of the workflow on the continue workflow event trigger.

```csharp
Step step2a = new()
{
    Name = "Step2a",
    FunctionDelegateName = "MiddleDelegate",
    ChildrenSteps = ["Step3"],
    Condition = null,
    WaitFor = ("event1", (_) => {
        Console.WriteLine("event1 completed");
        return true;
    })
};
```

The workflow can be continued by calling the ContinueWorkflowInstance method on the workflow executor with the Workflow Instance and name
of the continue workflow event trigger passed to it.

```csharp
await _workflowExecutor.ContinueWorkflowInstance(workflowInstance, eventTriggerName);
```

## License

This project is licensed under the MIT License.