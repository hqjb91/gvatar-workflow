using GvatarWorkflow.Entities.Interfaces;

namespace Sample1.Delegate;

public class EndWorkflowDelegate :IDelegate
{
    public object? Execute(object input)
    {
        Console.WriteLine($"End Result is : {input}");
        Console.WriteLine("End");

        return null;
    }
}
