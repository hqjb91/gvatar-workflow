using System.Text.Json;
using GvatarWorkflow.Entities;
using GvatarWorkflow.Providers.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GvatarWorkflow.Providers;

public sealed class EntityFrameworkPersistenceProvider(WorkflowDbContext dbContext) : IPersistenceProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly WorkflowDbContext _dbContext = dbContext;

    public Task<Guid> CreateNewWorkflowInstance(WorkflowDefinition workflowDefinition)
    {
        return CreateNewWorkflowInstance(workflowDefinition, null);
    }

    public async Task<Guid> CreateNewWorkflowInstance(WorkflowDefinition workflowDefinition, object? input)
    {
        Guid newGuid = Guid.NewGuid();
        WorkflowInstance newWorkflowInstance = new("New", [], [workflowDefinition.Steps[0].Id], input, workflowDefinition)
        {
            Id = newGuid
        };

        _dbContext.WorkflowInstances.Add(ToRecord(newWorkflowInstance));
        await _dbContext.SaveChangesAsync();

        return newGuid;
    }

    public async Task<IEnumerable<WorkflowInstance>> GetAllWorkflowInstancesByIds(IEnumerable<Guid> ids)
    {
        List<WorkflowInstanceRecord> records = await _dbContext.WorkflowInstances
            .Where(record => ids.Contains(record.Id))
            .ToListAsync();

        return records.Select(ToWorkflowInstance).ToList();
    }

    public async Task<WorkflowInstance> GetWorkflowInstanceById(Guid workflowInstanceId)
    {
        WorkflowInstanceRecord? record = await _dbContext.WorkflowInstances
            .FirstOrDefaultAsync(instance => instance.Id == workflowInstanceId);

        if (record is null)
        {
            throw new KeyNotFoundException($"No workflow instance found for id {workflowInstanceId}.");
        }

        return ToWorkflowInstance(record);
    }

    public async Task PersistWorkflowInstance(WorkflowInstance workflowInstance)
    {
        WorkflowInstanceRecord? record = await _dbContext.WorkflowInstances
            .FirstOrDefaultAsync(instance => instance.Id == workflowInstance.Id);

        if (record is null)
        {
            throw new KeyNotFoundException($"No workflow instance found for id {workflowInstance.Id}.");
        }

        UpdateRecord(record, workflowInstance);
        await _dbContext.SaveChangesAsync();
    }

    private static WorkflowInstanceRecord ToRecord(WorkflowInstance workflowInstance)
    {
        string definitionJson = JsonSerializer.Serialize(WorkflowDefinitionRecord.FromDefinition(workflowInstance.WorkflowDefinition), JsonOptions);
        return new WorkflowInstanceRecord
        {
            Id = workflowInstance.Id,
            Status = workflowInstance.Status,
            CurrentStepObjectContextJson = SerializeOptional(workflowInstance.CurrentStepObjectContext),
            CurrentStepObjectContextType = workflowInstance.CurrentStepObjectContext?.GetType().AssemblyQualifiedName,
            PreviousCompletedStepIdsJson = JsonSerializer.Serialize(workflowInstance.PreviousCompletedStepIds, JsonOptions),
            NextPendingStepIdsJson = JsonSerializer.Serialize(workflowInstance.NextPendingStepIds, JsonOptions),
            WorkflowDefinitionJson = definitionJson
        };
    }

    private static void UpdateRecord(WorkflowInstanceRecord record, WorkflowInstance workflowInstance)
    {
        record.Status = workflowInstance.Status;
        record.CurrentStepObjectContextJson = SerializeOptional(workflowInstance.CurrentStepObjectContext);
        record.CurrentStepObjectContextType = workflowInstance.CurrentStepObjectContext?.GetType().AssemblyQualifiedName;
        record.PreviousCompletedStepIdsJson = JsonSerializer.Serialize(workflowInstance.PreviousCompletedStepIds, JsonOptions);
        record.NextPendingStepIdsJson = JsonSerializer.Serialize(workflowInstance.NextPendingStepIds, JsonOptions);
        record.WorkflowDefinitionJson = JsonSerializer.Serialize(WorkflowDefinitionRecord.FromDefinition(workflowInstance.WorkflowDefinition), JsonOptions);
    }

    private static WorkflowInstance ToWorkflowInstance(WorkflowInstanceRecord record)
    {
        WorkflowDefinitionRecord definitionRecord = JsonSerializer.Deserialize<WorkflowDefinitionRecord>(record.WorkflowDefinitionJson, JsonOptions)
            ?? throw new InvalidOperationException("Workflow definition data is missing.");

        WorkflowDefinition workflowDefinition = definitionRecord.ToDefinition();
        object? context = DeserializeOptional(record.CurrentStepObjectContextJson, record.CurrentStepObjectContextType);
        List<Guid> previous = JsonSerializer.Deserialize<List<Guid>>(record.PreviousCompletedStepIdsJson, JsonOptions) ?? [];
        List<Guid> next = JsonSerializer.Deserialize<List<Guid>>(record.NextPendingStepIdsJson, JsonOptions) ?? [];

        return new WorkflowInstance(record.Status, previous, next, context, workflowDefinition)
        {
            Id = record.Id
        };
    }

    private static string? SerializeOptional(object? value)
    {
        return value is null ? null : JsonSerializer.Serialize(value, JsonOptions);
    }

    private static object? DeserializeOptional(string? json, string? typeName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        Type? type = string.IsNullOrWhiteSpace(typeName) ? null : Type.GetType(typeName);
        return type is null ? JsonSerializer.Deserialize<object>(json, JsonOptions) : JsonSerializer.Deserialize(json, type, JsonOptions);
    }
}
