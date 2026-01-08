using GvatarWorkflow.Entities.Interfaces;
using System.Reflection;
using System;
using System.Linq;

namespace GvatarWorkflow.Context;

public class DelegateContext
{
    private IEnumerable<Type>? _types;

    public void InitialPopulationOfAssemblyTypes()
    {
        _types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(GetLoadableTypes)
            .Where(type => typeof(IDelegate).IsAssignableFrom(type) && type is { IsClass: true, IsAbstract: false });
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

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(type => type is not null)!;
        }
    }
}
