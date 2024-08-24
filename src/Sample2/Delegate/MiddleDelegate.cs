using GvatarWorkflow.Entities.Interfaces;

namespace Sample1.Delegate;

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
