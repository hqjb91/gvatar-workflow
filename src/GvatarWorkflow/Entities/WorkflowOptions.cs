using GvatarWorkflow.Entities.Interfaces;
using GvatarWorkflow.Providers;
using GvatarWorkflow.Providers.Interfaces;
using GvatarWorkflow.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GvatarWorkflow.Entities;

public class WorkflowOptions(IServiceCollection services) : IWorkflowOptions
{
    public Func<IServiceProvider, IPersistenceProvider> PersistenceFactory = new(serviceProvider => new InMemoryPersistenceProvider(serviceProvider.GetService<SingletonInMemoryPersistenceProvider>() ?? throw new Exception("Workflow PersistenceProvider is required.")));
    public Func<IServiceProvider, IQueueProvider> QueueFactory = new(serviceProvider => new InMemoryQueueProvider(serviceProvider.GetService<IWorkflowExecutor>() ?? throw new Exception("Workflow Executor is required.")));

    public IServiceCollection Services { get; set; } = services;

    public void UsePersistenceProvider(Func<IServiceProvider, IPersistenceProvider> factory)
    {
        PersistenceFactory = factory;
    }

    public void UseQueueProvider(Func<IServiceProvider, IQueueProvider> factory)
    {
        QueueFactory = factory;
    }
}
