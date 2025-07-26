using CustomerCreditClassifier.Domain.Events.NextService;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CustomerCreditClassifier.Infrastructure.MassTransit.StateMachines;

public class NextServiceStateMachine : MassTransitStateMachine<SagaState>
{
    private readonly ILogger<NextServiceStateMachine> _logger;
    
    public State NextServiceRequested { get; private set; }
    public State NextServiceSucceeded { get; private set; }
    public State NextServiceFailed { get; private set; }

    public Event<NextServiceRequested> NextServiceRequestStarted { get; private set; }
    public Event<NextServiceSucceeded> NextServiceRequestSucceeded { get; private set; }
    public Event<NextServiceFailed> NextServiceRequestFailed { get; private set; }

    public NextServiceStateMachine(ILogger<NextServiceStateMachine> logger)
    {
        _logger = logger;
        
        InstanceState(x => x.CurrentState);

        Initially(
            When(NextServiceRequestStarted)
                .Then(context =>
                {
                    context.Saga.CorrelationId = context.Message.CorrelationId;
                    context.Saga.CurrentState = context.Message.CurrentState;
                    context.Saga.Version = context.Message.Version;
                    context.Saga.Payload = context.Message.Payload;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    context.Saga.ErrorMessage = null;
                    
                    _logger.LogInformation($"Next Service process started for customer: {context.Saga.CorrelationId}");
                })
                .TransitionTo(NextServiceRequested)
        );

        During(NextServiceRequested,
            When(NextServiceRequestSucceeded)
                .Then(context =>
                {
                    context.Saga.Payload = context.Message.Payload;
                    
                    _logger.LogInformation($"Next Service process succeeded for customer: {context.Saga.CorrelationId}");
                })
                .TransitionTo(NextServiceSucceeded)
        );

        During(NextServiceRequested,
            When(NextServiceRequestFailed)
                .Then(context =>
                {
                    context.Saga.ErrorMessage = context.Message.ErrorMessage;
                    
                    _logger.LogInformation($"Next Service process failed: {context.Saga.CorrelationId}");
                })
                .TransitionTo(NextServiceFailed)
        );

        SetCompletedWhenFinalized();
    }
}