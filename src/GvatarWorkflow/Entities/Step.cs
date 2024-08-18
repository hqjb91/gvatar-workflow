using GvatarWorkflow.Entities.Interfaces;

namespace GvatarWorkflow.Entities;

public class Step : IStep
{
    public string Name { get; set; } = "";
    public string FunctionDelegateName { get; set; } = "";
    public List<string>? ChildrenSteps { get; set; } = [];
    public Func<object?, bool>? Condition { get; set; } = (_) => true;
    public bool ShouldRun()
    {
        return Condition is not null && Condition(null);
    }
}
