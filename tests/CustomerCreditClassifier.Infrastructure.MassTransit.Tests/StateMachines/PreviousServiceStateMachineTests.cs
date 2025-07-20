using CustomerCreditClassifier.Domain.Events.PreviousService;
using CustomerCreditClassifier.Infrastructure.MassTransit.StateMachines;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json.Nodes;
using FluentAssertions;
using Xunit;
using Bogus;

namespace CustomerCreditClassifier.Infrastructure.MassTransit.Tests.StateMachines;

public class PreviousServiceStateMachineTests : IAsyncLifetime
{
    private readonly Mock<ILogger<PreviousServiceStateMachine>> _loggerMock;
    private readonly PreviousServiceStateMachine _stateMachine;
    private readonly InMemorySagaRepository<SagaState> _repository;
    private readonly InMemoryTestHarness _harness;

    public PreviousServiceStateMachineTests()
    {
        _loggerMock = new Mock<ILogger<PreviousServiceStateMachine>>();
        _stateMachine = new PreviousServiceStateMachine(_loggerMock.Object);
        _repository = new InMemorySagaRepository<SagaState>();
        _harness = new InMemoryTestHarness();
        _harness.StateMachineSaga(_stateMachine, _repository);
    }

    public async Task InitializeAsync()
    {
        await _harness.Start();
    }

    public async Task DisposeAsync()
    {
        await _harness.Stop();
    }

    [Fact]
    public async Task Should_Transition_To_Requested_On_RequestStarted()
    {
        var correlationId = Guid.NewGuid();
        var payload = new JsonObject
        {
            ["FirstName"] = new Faker().Person.FirstName,
            ["LastName"] = new Faker().Person.LastName,
            ["Email"] = new Faker().Person.Email,
            ["Phone"] = new Faker().Person.Phone
        };

        await _harness.Bus.Publish(new PreviousServiceRequested
        {
            CorrelationId = correlationId,
            Payload = payload
        });

        var instance = await ((IQuerySagaRepository<SagaState>)_repository).ShouldContainSaga(x => x.CorrelationId == correlationId && x.CurrentState == _stateMachine.Requested.Name, TimeSpan.FromSeconds(2));
        Assert.NotNull(instance);
    }

    [Fact]
    public async Task Should_Transition_To_Succeeded_On_RequestSucceeded()
    {
        var correlationId = Guid.NewGuid();
        var payload = new JsonObject
        {
            ["FirstName"] = new Faker().Person.FirstName,
            ["LastName"] = new Faker().Person.LastName,
            ["Email"] = new Faker().Person.Email,
            ["Phone"] = new Faker().Person.Phone
        };

        await _harness.Bus.Publish(new PreviousServiceRequested
        {
            CorrelationId = correlationId,
            Payload = payload
        });

        await _harness.Bus.Publish(new PreviousServiceSucceeded
        {
            CorrelationId = correlationId,
            Payload = payload
        });

        var instance = await ((IQuerySagaRepository<SagaState>)_repository).ShouldContainSaga(x => x.CorrelationId == correlationId && x.CurrentState == _stateMachine.Succeeded.Name, TimeSpan.FromSeconds(2));
        Assert.NotNull(instance);
    }

    [Fact]
    public async Task Should_Transition_To_Failed_On_RequestFailed()
    {
        var correlationId = Guid.NewGuid();
        var payload = new JsonObject
        {
            ["FirstName"] = new Faker().Person.FirstName,
            ["LastName"] = new Faker().Person.LastName,
            ["Email"] = new Faker().Person.Email,
            ["Phone"] = new Faker().Person.Phone
        };
        var errorMessage = "Some error";

        await _harness.Bus.Publish(new PreviousServiceRequested
        {
            CorrelationId = correlationId,
            Payload = payload
        });

        await _harness.Bus.Publish(new PreviousServiceFailed
        {
            CorrelationId = correlationId,
            Payload = payload,
            ErrorMessage = errorMessage
        });

        var instance = await ((IQuerySagaRepository<SagaState>)_repository).ShouldContainSaga(x => x.CorrelationId == correlationId && x.CurrentState == _stateMachine.Failed.Name, TimeSpan.FromSeconds(2));
        Assert.NotNull(instance);
    }
}
