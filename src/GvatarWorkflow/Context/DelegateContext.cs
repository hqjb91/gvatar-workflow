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
        if (_types is null)
        {
            throw new InvalidOperationException("DelegateContext has not been initialized. Call InitialPopulationOfAssemblyTypes first.");
        }

        if (condition == null || condition(input))
        {
            Type? delegateToInvoke = _types.FirstOrDefault(type => type.Name == delegateName);
            if (delegateToInvoke is null)
            {
                throw new InvalidOperationException($"Delegate '{delegateName}' was not found. Ensure it is loaded and implements {nameof(IDelegate)}.");
            }

            object? delegateInstance = Activator.CreateInstance(delegateToInvoke);
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
