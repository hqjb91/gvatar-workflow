namespace GvatarWorkflow.Entities;

public class ActivityInstance(Guid stepId, string stepName)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StepId { get; set; } = stepId;
    public string StepName { get; set; } = stepName;
    public string Status { get; set; } = "Pending";
    public int Attempt { get; set; } = 0;
    public int CompensationAttempt { get; set; } = 0;
    public Guid? CompensationStepId { get; set; }
    public DateTimeOffset? CompensationScheduledAtUtc { get; set; }
    public DateTimeOffset? CompensationCompletedAtUtc { get; set; }
    public bool CompensationExecuted { get; set; }
    public DateTimeOffset? StartedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public string? WaitingForEvent { get; set; }
    public object? Output { get; set; }
    public string? ErrorMessage { get; set; }
}
