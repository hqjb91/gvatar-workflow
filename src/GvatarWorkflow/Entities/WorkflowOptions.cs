using GvatarWorkflow.Entities.Interfaces;
using GvatarWorkflow.Providers;
using GvatarWorkflow.Providers.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GvatarWorkflow.Entities;

public class WorkflowOptions(IServiceCollection services) : IWorkflowOptions
{
    public Func<IServiceProvider, IPersistenceProvider> PersistenceFactory = new(serviceProvider => new InMemoryPersistenceProvider(serviceProvider.GetService<SingletonInMemoryPersistenceProvider>() ?? throw new Exception("Workflow PersistenceProvider is required.")));
    public Func<IServiceProvider, IWorkflowEventStore> EventStoreFactory = new(serviceProvider => new InMemoryWorkflowEventStore(serviceProvider.GetService<SingletonInMemoryWorkflowEventStore>() ?? throw new Exception("Workflow EventStore is required.")));
    public Func<IServiceProvider, IQueueProvider> QueueFactory = new(serviceProvider => new InMemoryQueueProvider(serviceProvider.GetService<IServiceScopeFactory>() ?? throw new Exception("ServiceScopeFactory is required.")));

    public IServiceCollection Services { get; set; } = services;

    public void UsePersistenceProvider(Func<IServiceProvider, IPersistenceProvider> factory)
    {
        PersistenceFactory = factory;
    }

    public void UseEventStore(Func<IServiceProvider, IWorkflowEventStore> factory)
    {
        EventStoreFactory = factory;
    }

    public void UseQueueProvider(Func<IServiceProvider, IQueueProvider> factory)
    {
        QueueFactory = factory;
    }
}
