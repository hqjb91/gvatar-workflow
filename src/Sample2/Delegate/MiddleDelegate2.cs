using GvatarWorkflow.Entities.Interfaces;

namespace Sample1.Delegate;

public class MiddleDelegate2 : IDelegate
{
    public object Execute(object input)
    {
        Console.WriteLine($"Middle 2 step will take output from Middle step and multiply by 3");
        int intputAsNumber = (int)input;
        int output = intputAsNumber * 3;

        return output;
    }
}
