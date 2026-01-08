using Microsoft.EntityFrameworkCore;

namespace GvatarWorkflow.Providers;

public sealed class WorkflowDbContext(DbContextOptions<WorkflowDbContext> options) : DbContext(options)
{
    public DbSet<WorkflowInstanceRecord> WorkflowInstances => Set<WorkflowInstanceRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkflowInstanceRecord>()
            .HasKey(instance => instance.Id);
    }
}

public sealed class WorkflowInstanceRecord
{
    public Guid Id { get; set; }
    public string Status { get; set; } = "";
    public string? CurrentStepObjectContextJson { get; set; }
    public string? CurrentStepObjectContextType { get; set; }
    public string PreviousCompletedStepNamesJson { get; set; } = "[]";
    public string NextPendingStepNamesJson { get; set; } = "[]";
    public string WorkflowDefinitionJson { get; set; } = "";
}
