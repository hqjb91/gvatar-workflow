using Microsoft.EntityFrameworkCore;

namespace GvatarWorkflow.Providers;

public sealed class WorkflowDbContext(DbContextOptions<WorkflowDbContext> options) : DbContext(options)
{
    public DbSet<WorkflowInstanceRecord> WorkflowInstances => Set<WorkflowInstanceRecord>();
    public DbSet<WorkflowEventWaitRecord> WorkflowEventWaits => Set<WorkflowEventWaitRecord>();
    public DbSet<WorkflowEventRecordEntry> WorkflowEvents => Set<WorkflowEventRecordEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkflowInstanceRecord>()
            .HasKey(instance => instance.Id);
        modelBuilder.Entity<WorkflowEventWaitRecord>()
            .HasKey(wait => wait.Id);
        modelBuilder.Entity<WorkflowEventRecordEntry>()
            .HasKey(evt => evt.Id);
    }
}

public sealed class WorkflowInstanceRecord
{
    public Guid Id { get; set; }
    public string Status { get; set; } = "";
    public string? CurrentStepObjectContextJson { get; set; }
    public string? CurrentStepObjectContextType { get; set; }
    public string PreviousCompletedStepIdsJson { get; set; } = "[]";
    public string NextPendingStepIdsJson { get; set; } = "[]";
    public string WorkflowDefinitionJson { get; set; } = "";
}
