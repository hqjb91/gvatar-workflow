using System.Text.Json;
using GvatarWorkflow.Entities;
using GvatarWorkflow.Providers.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GvatarWorkflow.Providers;

public sealed class EntityFrameworkWorkflowEventStore(WorkflowDbContext dbContext) : IWorkflowEventStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly WorkflowDbContext _dbContext = dbContext;

    public async Task<WorkflowEventWait?> GetWaitForStep(Guid workflowInstanceId, Guid stepId)
    {
        WorkflowEventWaitRecord? record = await _dbContext.WorkflowEventWaits
            .FirstOrDefaultAsync(wait => wait.WorkflowInstanceId == workflowInstanceId && wait.StepId == stepId);

        return record is null ? null : ToWait(record);
    }

    public async Task<WorkflowEventWait> CreateWait(WorkflowEventWait wait)
    {
        WorkflowEventWaitRecord record = ToRecord(wait);
        _dbContext.WorkflowEventWaits.Add(record);
        await _dbContext.SaveChangesAsync();
        return wait;
    }

    public async Task<IReadOnlyList<WorkflowEventWait>> GetWaitsByEvent(string eventName, IReadOnlyCollection<WorkflowEventKey> correlationKeys)
    {
        List<WorkflowEventWaitRecord> records = await _dbContext.WorkflowEventWaits
            .Where(wait => wait.EventName == eventName)
            .ToListAsync();

        return records
            .Select(ToWait)
            .Where(wait => CorrelationKeysMatch(wait.CorrelationKeys, correlationKeys))
            .ToList();
    }

    public async Task RemoveWait(Guid waitId)
    {
        WorkflowEventWaitRecord? existing = await _dbContext.WorkflowEventWaits
            .FirstOrDefaultAsync(wait => wait.Id == waitId);

        if (existing is not null)
        {
            _dbContext.WorkflowEventWaits.Remove(existing);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<WorkflowEventRecord> RecordEvent(WorkflowEventRecord workflowEvent)
    {
        WorkflowEventRecordEntry record = ToRecord(workflowEvent);
        _dbContext.WorkflowEvents.Add(record);
        await _dbContext.SaveChangesAsync();
        return workflowEvent;
    }

    public async Task<WorkflowEventRecord?> FindMatchingEvent(string eventName, IReadOnlyCollection<WorkflowEventKey> correlationKeys)
    {
        List<WorkflowEventRecordEntry> records = await _dbContext.WorkflowEvents
            .Where(evt => evt.EventName == eventName)
            .ToListAsync();

        return records
            .Select(ToEvent)
            .FirstOrDefault(evt => CorrelationKeysMatch(evt.CorrelationKeys, correlationKeys));
    }

    public async Task RemoveEvent(Guid eventId)
    {
        WorkflowEventRecordEntry? existing = await _dbContext.WorkflowEvents
            .FirstOrDefaultAsync(evt => evt.Id == eventId);

        if (existing is not null)
        {
            _dbContext.WorkflowEvents.Remove(existing);
            await _dbContext.SaveChangesAsync();
        }
    }

    private static WorkflowEventWaitRecord ToRecord(WorkflowEventWait wait)
    {
        return new WorkflowEventWaitRecord
        {
            Id = wait.Id,
            WorkflowInstanceId = wait.WorkflowInstanceId,
            StepId = wait.StepId,
            StepName = wait.StepName,
            EventName = wait.EventName,
            CorrelationKeysJson = JsonSerializer.Serialize(wait.CorrelationKeys, JsonOptions),
            CreatedAtUtc = wait.CreatedAtUtc
        };
    }

    private static WorkflowEventWait ToWait(WorkflowEventWaitRecord record)
    {
        List<WorkflowEventKey> keys = JsonSerializer.Deserialize<List<WorkflowEventKey>>(record.CorrelationKeysJson, JsonOptions) ?? [];
        return new WorkflowEventWait
        {
            Id = record.Id,
            WorkflowInstanceId = record.WorkflowInstanceId,
            StepId = record.StepId,
            StepName = record.StepName,
            EventName = record.EventName,
            CorrelationKeys = keys,
            CreatedAtUtc = record.CreatedAtUtc
        };
    }

    private static WorkflowEventRecordEntry ToRecord(WorkflowEventRecord workflowEvent)
    {
        return new WorkflowEventRecordEntry
        {
            Id = workflowEvent.Id,
            EventName = workflowEvent.EventName,
            CorrelationKeysJson = JsonSerializer.Serialize(workflowEvent.CorrelationKeys, JsonOptions),
            RecordedAtUtc = workflowEvent.RecordedAtUtc
        };
    }

    private static WorkflowEventRecord ToEvent(WorkflowEventRecordEntry record)
    {
        List<WorkflowEventKey> keys = JsonSerializer.Deserialize<List<WorkflowEventKey>>(record.CorrelationKeysJson, JsonOptions) ?? [];
        return new WorkflowEventRecord
        {
            Id = record.Id,
            EventName = record.EventName,
            CorrelationKeys = keys,
            RecordedAtUtc = record.RecordedAtUtc
        };
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
