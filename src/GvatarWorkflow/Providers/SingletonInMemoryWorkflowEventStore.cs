using GvatarWorkflow.Entities;
using GvatarWorkflow.Providers.Interfaces;

namespace GvatarWorkflow.Providers;

public sealed class SingletonInMemoryWorkflowEventStore : IWorkflowEventStore
{
    private readonly List<WorkflowEventWait> _waits = [];
    private readonly List<WorkflowEventRecord> _events = [];
    private readonly object _lock = new();

    public Task<WorkflowEventWait?> GetWaitForStep(Guid workflowInstanceId, Guid stepId)
    {
        lock (_lock)
        {
            return Task.FromResult(_waits.FirstOrDefault(wait => wait.WorkflowInstanceId == workflowInstanceId && wait.StepId == stepId));
        }
    }

    public Task<WorkflowEventWait> CreateWait(WorkflowEventWait wait)
    {
        lock (_lock)
        {
            _waits.Add(wait);
            return Task.FromResult(wait);
        }
    }

    public Task<IReadOnlyList<WorkflowEventWait>> GetWaitsByEvent(string eventName, IReadOnlyCollection<WorkflowEventKey> correlationKeys)
    {
        lock (_lock)
        {
            IReadOnlyList<WorkflowEventWait> waits = _waits
                .Where(wait => wait.EventName == eventName && CorrelationKeysMatch(wait.CorrelationKeys, correlationKeys))
                .ToList();
            return Task.FromResult(waits);
        }
    }

    public Task RemoveWait(Guid waitId)
    {
        lock (_lock)
        {
            WorkflowEventWait? existing = _waits.FirstOrDefault(wait => wait.Id == waitId);
            if (existing is not null)
            {
                _waits.Remove(existing);
            }
        }

        return Task.CompletedTask;
    }

    public Task<WorkflowEventRecord> RecordEvent(WorkflowEventRecord workflowEvent)
    {
        lock (_lock)
        {
            _events.Add(workflowEvent);
            return Task.FromResult(workflowEvent);
        }
    }

    public Task<WorkflowEventRecord?> FindMatchingEvent(string eventName, IReadOnlyCollection<WorkflowEventKey> correlationKeys)
    {
        lock (_lock)
        {
            WorkflowEventRecord? match = _events
                .FirstOrDefault(evt => evt.EventName == eventName && CorrelationKeysMatch(evt.CorrelationKeys, correlationKeys));
            return Task.FromResult(match);
        }
    }

    public Task RemoveEvent(Guid eventId)
    {
        lock (_lock)
        {
            WorkflowEventRecord? existing = _events.FirstOrDefault(evt => evt.Id == eventId);
            if (existing is not null)
            {
                _events.Remove(existing);
            }
        }

        return Task.CompletedTask;
    }

    private static bool CorrelationKeysMatch(IReadOnlyCollection<WorkflowEventKey> expected, IReadOnlyCollection<WorkflowEventKey> actual)
    {
        if (expected.Count != actual.Count)
        {
            return false;
        }

        Dictionary<string, string> expectedMap = expected.ToDictionary(key => key.Key, key => key.Value, StringComparer.Ordinal);
        foreach (WorkflowEventKey actualKey in actual)
        {
            if (!expectedMap.TryGetValue(actualKey.Key, out string? value) || value != actualKey.Value)
            {
                return false;
            }
        }

        return true;
    }
}
