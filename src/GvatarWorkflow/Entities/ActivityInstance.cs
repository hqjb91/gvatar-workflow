namespace GvatarWorkflow.Entities;

public class ActivityInstance(string stepName)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string StepName { get; set; } = stepName;
    public string Status { get; set; } = "Pending";
    public int Attempt { get; set; } = 0;
    public DateTimeOffset? StartedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public string? WaitingForEvent { get; set; }
    public object? Output { get; set; }
    public string? ErrorMessage { get; set; }
}
