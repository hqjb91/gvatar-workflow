namespace GvatarWorkflow.Entities;

public class Step
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string FunctionDelegateName { get; set; } = "";
    public List<Transition> Transitions { get; set; } = [];
    public (string, Func<object?, bool>)? WaitFor { get; set; }
}
