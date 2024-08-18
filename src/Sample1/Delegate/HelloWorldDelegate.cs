using GvatarWorkflow.Entities.Interfaces;

namespace Sample1.Delegate;

public class HelloWorldDelegate : IDelegate
{
    public object Execute(object input)
    {
        Console.WriteLine($"Hello");

        return input;
    }
}
