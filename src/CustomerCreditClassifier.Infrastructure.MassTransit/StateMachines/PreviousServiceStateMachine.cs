using CustomerCreditClassifier.Domain.Events.Acl;
using CustomerCreditClassifier.Domain.Events.PreviousService;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CustomerCreditClassifier.Infrastructure.MassTransit.StateMachines;

public class PreviousServiceStateMachine : MassTransitStateMachine<SagaState>
{
    private readonly ILogger<PreviousServiceStateMachine> _logger;
    
    public State Requested { get; private set; }
    public State Succeeded { get; private set; }
    public State Failed { get; private set; }

    public Event<PreviousServiceRequested> RequestStarted { get; private set; }
    public Event<PreviousServiceSucceeded> RequestSucceeded { get; private set; }
    public Event<PreviousServiceFailed> RequestFailed { get; private set; }

    public PreviousServiceStateMachine(ILogger<PreviousServiceStateMachine> logger)
    {
        _logger = logger;
        
        InstanceState(x => x.CurrentState);

        Initially(
            When(RequestStarted)
                .Then(context =>
                {
                    context.Saga.Payload = context.Message.Payload;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    _logger.LogInformation($"Previous Service process started for customer: {context.Saga.CorrelationId}");
                })
                .TransitionTo(Requested)
        );

        During(Requested,
            When(RequestSucceeded)
                .Then(context =>
                {
                    context.Saga.Payload = context.Message.Payload;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    _logger.LogInformation($"Previous Service process succeeded for customer: {context.Saga.CorrelationId}");
                })
                .Publish(context => new AclRequested
                {
                    CorrelationId = context.Saga.CorrelationId,
                    Payload = context.Message.Payload,
                    UpdatedAt = DateTime.UtcNow
                })
                .TransitionTo(Succeeded)
        );

        During(Requested,
            When(RequestFailed)
                .Then(context =>
                {
                    context.Saga.Payload = context.Message.Payload;
                    context.Saga.ErrorMessage = context.Message.ErrorMessage;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    _logger.LogInformation($"Previous Service process failed: {context.Saga.CorrelationId}");
                })
                .TransitionTo(Failed)
        );

        SetCompletedWhenFinalized();
    }
}