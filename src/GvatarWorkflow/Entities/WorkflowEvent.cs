namespace GvatarWorkflow.Entities;

public sealed record WorkflowEventKey(string Key, string Value);

public sealed class WorkflowEventWait
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkflowInstanceId { get; set; }
    public Guid StepId { get; set; }
    public string StepName { get; set; } = "";
    public string EventName { get; set; } = "";
    public List<WorkflowEventKey> CorrelationKeys { get; set; } = [];
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class WorkflowEventRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventName { get; set; } = "";
    public List<WorkflowEventKey> CorrelationKeys { get; set; } = [];
    public DateTimeOffset RecordedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
