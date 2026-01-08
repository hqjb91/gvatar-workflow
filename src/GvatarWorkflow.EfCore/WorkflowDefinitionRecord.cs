using GvatarWorkflow.Entities;

namespace GvatarWorkflow.Providers;

public sealed class WorkflowDefinitionRecord
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int Version { get; set; }
    public List<StepRecord> Steps { get; set; } = [];

    public static WorkflowDefinitionRecord FromDefinition(WorkflowDefinition workflowDefinition)
    {
        return new WorkflowDefinitionRecord
        {
            Id = workflowDefinition.Id,
            Name = workflowDefinition.Name,
            Description = workflowDefinition.Description,
            Version = workflowDefinition.Version,
            Steps = workflowDefinition.Steps.Select(StepRecord.FromStep).ToList()
        };
    }

    public WorkflowDefinition ToDefinition()
    {
        return new WorkflowDefinition
        {
            Id = Id,
            Name = Name,
            Description = Description,
            Version = Version,
            Steps = Steps.Select(stepRecord => stepRecord.ToStep()).ToList()
        };
    }
}

public sealed class StepRecord
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string FunctionDelegateName { get; set; } = "";
    public List<TransitionRecord> Transitions { get; set; } = [];

    public static StepRecord FromStep(Step step)
    {
        return new StepRecord
        {
            Id = step.Id,
            Name = step.Name,
            FunctionDelegateName = step.FunctionDelegateName,
            Transitions = step.Transitions.Select(TransitionRecord.FromTransition).ToList()
        };
    }

    public Step ToStep()
    {
        return new Step
        {
            Id = Id,
            Name = Name,
            FunctionDelegateName = FunctionDelegateName,
            Transitions = Transitions.Select(transition => transition.ToTransition()).ToList()
        };
    }
}

public sealed class TransitionRecord
{
    public Guid Id { get; set; }
    public Guid FromStepId { get; set; }
    public Guid ToStepId { get; set; }
    public TransitionConditionRecord? Condition { get; set; }
    public RetryPolicyRecord? RetryPolicy { get; set; }
    public Guid? CompensationStepId { get; set; }
    public bool OnFailure { get; set; }
    public int Order { get; set; }

    public static TransitionRecord FromTransition(Transition transition)
    {
        return new TransitionRecord
        {
            Id = transition.Id,
            FromStepId = transition.FromStepId,
            ToStepId = transition.ToStepId,
            Condition = transition.Condition is null ? null : TransitionConditionRecord.FromCondition(transition.Condition),
            RetryPolicy = transition.RetryPolicy is null ? null : RetryPolicyRecord.FromPolicy(transition.RetryPolicy),
            CompensationStepId = transition.CompensationStepId,
            OnFailure = transition.OnFailure,
            Order = transition.Order
        };
    }

    public Transition ToTransition()
    {
        return new Transition
        {
            Id = Id,
            FromStepId = FromStepId,
            ToStepId = ToStepId,
            Condition = Condition?.ToCondition(),
            RetryPolicy = RetryPolicy?.ToPolicy(),
            CompensationStepId = CompensationStepId,
            OnFailure = OnFailure,
            Order = Order
        };
    }
}

public sealed class TransitionConditionRecord
{
    public TransitionConditionType Type { get; set; } = TransitionConditionType.Always;
    public string? Value { get; set; }

    public static TransitionConditionRecord FromCondition(TransitionCondition condition)
    {
        return new TransitionConditionRecord
        {
            Type = condition.Type,
            Value = condition.Value
        };
    }

    public TransitionCondition ToCondition()
    {
        return new TransitionCondition
        {
            Type = Type,
            Value = Value
        };
    }
}

public sealed class RetryPolicyRecord
{
    public int MaxAttempts { get; set; }

    public static RetryPolicyRecord FromPolicy(RetryPolicy retryPolicy)
    {
        return new RetryPolicyRecord
        {
            MaxAttempts = retryPolicy.MaxAttempts
        };
    }

    public RetryPolicy ToPolicy()
    {
        return new RetryPolicy
        {
            MaxAttempts = MaxAttempts
        };
    }
}
