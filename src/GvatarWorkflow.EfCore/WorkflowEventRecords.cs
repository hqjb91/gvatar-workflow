namespace GvatarWorkflow.Providers;

public sealed class WorkflowEventWaitRecord
{
    public Guid Id { get; set; }
    public Guid WorkflowInstanceId { get; set; }
    public Guid StepId { get; set; }
    public string StepName { get; set; } = "";
    public string EventName { get; set; } = "";
    public string CorrelationKeysJson { get; set; } = "[]";
    public DateTimeOffset CreatedAtUtc { get; set; }
}

public sealed class WorkflowEventRecordEntry
{
    public Guid Id { get; set; }
    public string EventName { get; set; } = "";
    public string CorrelationKeysJson { get; set; } = "[]";
    public DateTimeOffset RecordedAtUtc { get; set; }
}
