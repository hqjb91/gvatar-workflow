using GvatarWorkflow.Context;
using GvatarWorkflow.Entities;
using GvatarWorkflow.Entities.Interfaces;
using GvatarWorkflow.Providers;
using GvatarWorkflow.Providers.Interfaces;
using GvatarWorkflow.Services;
using GvatarWorkflow.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GvatarWorkflow;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddGvatarWorkflow(this IServiceCollection services, Action<IWorkflowOptions> configureActions)
    {
        WorkflowOptions options = new(services);
        configureActions?.Invoke(options);

        services.AddTransient<IWorkflowService, WorkflowService>();
        services.AddTransient<IWorkflowExecutor, WorkflowExecutor>();
        services.AddScoped<IWorkflowDefinitionBuilder, WorkflowDefinitionBuilder>();
        services.AddSingleton<SingletonInMemoryPersistenceProvider>();
        services.AddTransient<IPersistenceProvider>(options.PersistenceFactory);
        services.AddSingleton<IQueueProvider>(options.QueueFactory);
        services.AddSingleton<DelegateContext>(serviceProvider =>
        {
            DelegateContext delegateContext = new();
            delegateContext.InitialPopulationOfAssemblyTypes();
            return delegateContext;
        });

        return services;
    }
}
