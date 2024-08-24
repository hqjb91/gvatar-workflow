using GvatarWorkflow.Entities.Interfaces;
using System.Reflection;

namespace GvatarWorkflow.Context;

public class DelegateContext
{
    private IEnumerable<Type>? _types;

    public void InitialPopulationOfAssemblyTypes()
    {
        Assembly? currentAssembly = Assembly.GetEntryAssembly();
        _types = currentAssembly?.GetTypes()
                                .Where(type => typeof(IDelegate).IsAssignableFrom(type) && type.IsClass);
    }

    public object? InvokeDelegate(string delegateName)
    {
        return InvokeDelegate(delegateName, null);
    }

    public object? InvokeDelegate(string delegateName, object? input)
    {
        return InvokeDelegate(delegateName, input, null);
    }

    public object? InvokeDelegate(string delegateName, object? input, Func<object?, bool>? condition)
    {
        if (condition == null || condition(input))
        {
            Type? delegateToInvoke = _types?
                                    .Where(type => type.Name == delegateName).First();
            object? delegateInstance = (delegateToInvoke is not null) ? Activator.CreateInstance(delegateToInvoke) : null;
            MethodInfo? executeMethod = delegateToInvoke?.GetMethod("Execute");
            return executeMethod?.Invoke(delegateInstance, [input]);
        }

        return input;
    }
}
