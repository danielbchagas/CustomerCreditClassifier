using CustomerCreditClassifier.Domain.Events;
using CustomerCreditClassifier.Domain.Events.NextService;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CustomerCreditClassifier.Infrastructure.MassTransit.StateMachines;

public class NextServiceStateMachine : MassTransitStateMachine<SagaState>
{
    private readonly ILogger<NextServiceStateMachine> _logger;
    
    public State Requested { get; private set; }
    public State Succeeded { get; private set; }
    public State Failed { get; private set; }

    public Event<NextServiceRequested> RequestStarted { get; private set; }
    public Event<NextServiceSucceeded> RequestSucceeded { get; private set; }
    public Event<NextServiceFailed> RequestFailed { get; private set; }

    public NextServiceStateMachine(ILogger<NextServiceStateMachine> logger)
    {
        _logger = logger;
        
        InstanceState(x => x.CurrentState);

        Initially(
            When(RequestStarted)
                .Then(context =>
                {
                    context.Saga.Payload = context.Message.Payload;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    _logger.LogInformation($"Next Service process started for customer: {context.Saga.CorrelationId}");
                })
                .TransitionTo(Requested)
        );

        During(Requested,
            When(RequestSucceeded)
                .Then(context =>
                {
                    context.Saga.Payload = context.Message.Payload;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    _logger.LogInformation($"Next Service process succeeded for customer: {context.Saga.CorrelationId}");
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
                    _logger.LogInformation($"Next Service process failed: {context.Saga.CorrelationId}");
                })
                .TransitionTo(Failed)
        );

        SetCompletedWhenFinalized();
    }
}