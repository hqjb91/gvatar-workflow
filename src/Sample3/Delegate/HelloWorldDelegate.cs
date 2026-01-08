using GvatarWorkflow.Entities.Interfaces;

namespace Sample3.Delegate;

public class HelloWorldDelegate : IDelegate
{
    public object Execute(object input)
    {
        Console.WriteLine($"Hello");

        return input;
    }
}
