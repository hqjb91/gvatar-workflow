using System.Collections.Concurrent;

namespace GvatarWorkflow.Providers.Interfaces;

public interface IQueueProvider
{
    public Task<Guid> DequeueWork(QueueType queue);

    public Task QueueWork(Guid id, QueueType queue);

    public Task Start();

    public Task Stop();

    public void Dispose();
}

public enum QueueType {  Workflow = 0, Event = 1 }