using GvatarWorkflow.Entities.Interfaces;

namespace GvatarWorkflow.Entities;

public class Step : IStep
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string FunctionDelegateName { get; set; } = "";
    public List<Guid>? ChildrenSteps { get; set; } = [];
    public Func<object?, bool>? Condition { get; set; } = (_) => true;
    public (string, Func<object?, bool>)? WaitFor { get; set; }
    public bool ShouldRun(object? input)
    {
        return Condition is not null && Condition(input);
    }
}
