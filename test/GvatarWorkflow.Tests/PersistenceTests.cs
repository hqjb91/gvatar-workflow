using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GvatarWorkflow.Context;
using GvatarWorkflow.Entities;
using GvatarWorkflow.Entities.Interfaces;
using GvatarWorkflow.Providers.Interfaces;
using GvatarWorkflow.Services;
using Xunit;

namespace GvatarWorkflow.Tests;

public class PersistenceTests
{
    [Fact]
    public async Task ExecuteWorkflowInstance_PersistsUpdates()
    {
        DelegateContext delegateContext = new();
        delegateContext.InitialPopulationOfAssemblyTypes();

        TrackingPersistenceProvider persistenceProvider = new();
        WorkflowExecutor executor = new(persistenceProvider, delegateContext);

        WorkflowDefinition workflowDefinition = new()
        {
            Name = "Test",
            Description = "Test",
            Version = 1,
            Steps =
            [
                new Step
                {
                    Name = "Step1",
                    FunctionDelegateName = nameof(EchoDelegate),
                    ChildrenSteps = null,
                    Condition = _ => true
                }
            ]
        };

        Guid workflowInstanceId = await persistenceProvider.CreateNewWorkflowInstance(workflowDefinition, 5);

        await executor.ExecuteWorkflowInstance(workflowInstanceId);

        WorkflowInstance instance = await persistenceProvider.GetWorkflowInstanceById(workflowInstanceId);

        Assert.Equal("Completed", instance.Status);
        Assert.Contains(workflowDefinition.Steps[0].Id, instance.PreviousCompletedStepIds);
        Assert.True(persistenceProvider.PersistCount >= 2, "Expected workflow updates to be persisted more than once.");
    }

    private sealed class TrackingPersistenceProvider : IPersistenceProvider
    {
        private readonly Dictionary<Guid, WorkflowInstance> _instances = new();

        public int PersistCount { get; private set; }

        public Task<Guid> CreateNewWorkflowInstance(WorkflowDefinition workflowDefinition)
        {
            return CreateNewWorkflowInstance(workflowDefinition, null);
        }

        public Task<Guid> CreateNewWorkflowInstance(WorkflowDefinition workflowDefinition, object? input)
        {
            Guid newGuid = Guid.NewGuid();
            WorkflowInstance newWorkflowInstance = new("New", [], [workflowDefinition.Steps[0].Id], input, workflowDefinition)
            {
                Id = newGuid
            };
            _instances[newGuid] = newWorkflowInstance;
            return Task.FromResult(newGuid);
        }

        public Task<IEnumerable<WorkflowInstance>> GetAllWorkflowInstancesByIds(IEnumerable<Guid> ids)
        {
            return Task.FromResult<IEnumerable<WorkflowInstance>>(_instances.Values.Where(instance => ids.Contains(instance.Id)).ToList());
        }

        public Task<WorkflowInstance> GetWorkflowInstanceById(Guid workflowInstanceId)
        {
            return Task.FromResult(_instances[workflowInstanceId]);
        }

        public Task PersistWorkflowInstance(WorkflowInstance workflowInstance)
        {
            PersistCount += 1;
            _instances[workflowInstance.Id] = workflowInstance;
            return Task.CompletedTask;
        }
    }

    private sealed class EchoDelegate : IDelegate
    {
        public object? Execute(object input)
        {
            return input;
        }
    }
}
