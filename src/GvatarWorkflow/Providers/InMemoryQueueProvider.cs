using GvatarWorkflow.Providers.Interfaces;
using GvatarWorkflow.Services.Interfaces;
using System.Collections.Concurrent;

namespace GvatarWorkflow.Providers;

public class InMemoryQueueProvider(IWorkflowExecutor workflowExecutor) : IQueueProvider
{
    private readonly IWorkflowExecutor _workflowExecutor = workflowExecutor;
    public Dictionary<QueueType, BlockingCollection<Guid>> _queues = new()
    {
        [QueueType.Workflow] = [],
        [QueueType.Event] = []
    };

    public Task<Guid> DequeueWork(QueueType queue)
    {
        if (_queues[queue].TryTake(out Guid id, 100))
            return Task.FromResult(id);
        return Task.FromResult<Guid>(Guid.Empty);
    }

    public Task QueueWork(Guid id, QueueType queue)
    {
        _queues[queue].TryAdd(id);
        return Task.CompletedTask;
    }

    public Task Start()
    {
        foreach (var queue in _queues.Values)
        {
            Thread thread = new(async () =>
            {
                foreach (var job in queue.GetConsumingEnumerable())
                {
                    await _workflowExecutor.ExecuteWorkflowInstance(job);
                }
            })
            {
                IsBackground = true
            };
            thread.Start();
        }

        return Task.CompletedTask;
    }

    public Task Stop()
    {
        foreach(var queue in _queues.Values)
        {
            queue.CompleteAdding();
        }
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Stop();
    }
}