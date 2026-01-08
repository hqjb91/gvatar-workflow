using GvatarWorkflow.Providers.Interfaces;

namespace GvatarWorkflow.Entities.Interfaces;

public interface IWorkflowOptions
{
    public void UsePersistenceProvider(Func<IServiceProvider, IPersistenceProvider> factory);
    public void UseEventStore(Func<IServiceProvider, IWorkflowEventStore> factory);
    public void UseQueueProvider(Func<IServiceProvider, IQueueProvider> factory);
}
