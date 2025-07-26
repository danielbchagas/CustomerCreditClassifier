using CustomerCreditClassifier.Domain.Events.Acl;
using CustomerCreditClassifier.Domain.Events.PreviousService;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CustomerCreditClassifier.Infrastructure.MassTransit.StateMachines;

public class PreviousServiceStateMachine : MassTransitStateMachine<SagaState>
{
    private readonly ILogger<PreviousServiceStateMachine> _logger;
    
    public State PreviousServiceRequested { get; private set; }
    public State PreviousServiceSucceeded { get; private set; }
    public State PreviousServiceFailed { get; private set; }

    public Event<PreviousServiceRequested> PreviousServiceRequestStarted { get; private set; }
    public Event<PreviousServiceSucceeded> PreviousServiceRequestSucceeded { get; private set; }
    public Event<PreviousServiceFailed> PreviousServiceRequestFailed { get; private set; }

    public PreviousServiceStateMachine(ILogger<PreviousServiceStateMachine> logger)
    {
        _logger = logger;
        
        InstanceState(x => x.CurrentState);

        Initially(
            When(PreviousServiceRequestStarted)
                .Then(context =>
                {
                    context.Saga.CorrelationId = context.Message.CorrelationId;
                    context.Saga.CurrentState = context.Message.CurrentState;
                    context.Saga.Version = context.Message.Version;
                    context.Saga.Payload = context.Message.Payload;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    context.Saga.ErrorMessage = null;
                    
                    _logger.LogInformation($"Previous Service process started for customer: {context.Saga.CorrelationId}");
                })
                .TransitionTo(PreviousServiceRequested)
        );

        During(PreviousServiceRequested,
            When(PreviousServiceRequestSucceeded)
                .Then(context =>
                {
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    
                    _logger.LogInformation($"Previous Service process succeeded for customer: {context.Saga.CorrelationId}");
                })
                .Publish(context => new AclRequested
                {
                    CorrelationId = context.Saga.CorrelationId,
                    CurrentState = context.Saga.CurrentState,
                    Version = context.Saga.Version,
                    Payload = context.Message.Payload,
                    UpdatedAt = DateTime.UtcNow,
                    ErrorMessage = null
                })
                .TransitionTo(PreviousServiceSucceeded)
        );

        During(PreviousServiceRequested,
            When(PreviousServiceRequestFailed)
                .Then(context =>
                {
                    context.Saga.ErrorMessage = context.Message.ErrorMessage;
                    
                    _logger.LogInformation($"Previous Service process failed: {context.Saga.CorrelationId}");
                })
                .TransitionTo(PreviousServiceFailed)
        );

        SetCompletedWhenFinalized();
    }
}