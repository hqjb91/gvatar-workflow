namespace GvatarWorkflow.Entities;

public enum TransitionConditionType
{
    Always,
    OutputTrue,
    OutputFalse,
    Equals,
    NotEquals,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual
}

public class TransitionCondition
{
    public TransitionConditionType Type { get; set; } = TransitionConditionType.Always;
    public string? Value { get; set; }

    public bool Evaluate(object? input)
    {
        switch (Type)
        {
            case TransitionConditionType.Always:
                return true;
            case TransitionConditionType.OutputTrue:
                return input is bool trueValue && trueValue;
            case TransitionConditionType.OutputFalse:
                return input is bool falseValue && !falseValue;
            case TransitionConditionType.Equals:
                return string.Equals(Value, input?.ToString(), StringComparison.OrdinalIgnoreCase);
            case TransitionConditionType.NotEquals:
                return !string.Equals(Value, input?.ToString(), StringComparison.OrdinalIgnoreCase);
            case TransitionConditionType.GreaterThan:
                return CompareNumeric(input, Value, comparison => comparison > 0);
            case TransitionConditionType.GreaterThanOrEqual:
                return CompareNumeric(input, Value, comparison => comparison >= 0);
            case TransitionConditionType.LessThan:
                return CompareNumeric(input, Value, comparison => comparison < 0);
            case TransitionConditionType.LessThanOrEqual:
                return CompareNumeric(input, Value, comparison => comparison <= 0);
            default:
                return false;
        }
    }

    private static bool CompareNumeric(object? input, string? value, Func<int, bool> predicate)
    {
        if (!TryConvertToDecimal(input, out decimal inputDecimal))
        {
            return false;
        }

        if (!TryConvertToDecimal(value, out decimal valueDecimal))
        {
            return false;
        }

        return predicate(inputDecimal.CompareTo(valueDecimal));
    }

    private static bool TryConvertToDecimal(object? value, out decimal result)
    {
        result = default;
        if (value is null)
        {
            return false;
        }

        if (value is decimal decimalValue)
        {
            result = decimalValue;
            return true;
        }

        if (value is IConvertible)
        {
            try
            {
                result = Convert.ToDecimal(value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        return decimal.TryParse(value.ToString(), out result);
    }
}

public class RetryPolicy
{
    public int MaxAttempts { get; set; } = 1;
}

public class Transition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FromStepId { get; set; }
    public Guid ToStepId { get; set; }
    public TransitionCondition? Condition { get; set; } = new();
    public RetryPolicy? RetryPolicy { get; set; }
    public Guid? CompensationStepId { get; set; }
    public bool OnFailure { get; set; }
    public int Order { get; set; }
}
