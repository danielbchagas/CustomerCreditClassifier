using CustomerCreditClassifier.Domain.Events.Acl;
using CustomerCreditClassifier.Domain.Events.NextService;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CustomerCreditClassifier.Infrastructure.MassTransit.StateMachines;

public class AclStateMachine : MassTransitStateMachine<SagaState>
{
    private readonly ILogger<AclStateMachine> _logger;
    
    public State AclRequested { get; private set; }
    public State AclSucceeded { get; private set; }
    public State AclFailed { get; private set; }
    
    public Event<AclRequested> AclRequestStarted { get; private set; }
    public Event<AclSucceeded> AclRequestSucceeded { get; private set; }
    public Event<AclFailed> AclRequestFailed { get; private set; }
    
    public AclStateMachine(ILogger<AclStateMachine> logger)
    {
        _logger = logger;
        
        InstanceState(x => x.CurrentState);

        Initially(
            When(AclRequestStarted)
                .Then(context =>
                {
                    context.Saga.CorrelationId = context.Message.CorrelationId;
                    context.Saga.CurrentState = context.Message.CurrentState;
                    context.Saga.Version = context.Message.Version;
                    context.Saga.Payload = context.Message.Payload;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    context.Saga.ErrorMessage = null;
                    
                    _logger.LogInformation($"ACL Process started for customer: {context.Saga.CorrelationId}");
                })
                .TransitionTo(AclRequested)
        );

        During(AclRequested,
            When(AclRequestSucceeded)
                .Then(context =>
                {
                    context.Saga.Payload = context.Message.Payload;
                    
                    _logger.LogInformation($"ACL Process succeeded for customer: {context.Saga.CorrelationId}");
                })
                .Publish(context => new NextServiceRequested
                {
                    CorrelationId = context.Saga.CorrelationId,
                    CurrentState = context.Saga.CurrentState,
                    Version = context.Saga.Version,
                    Payload = context.Message.Payload,
                    UpdatedAt = DateTime.UtcNow,
                    ErrorMessage = null
                })
                .TransitionTo(AclSucceeded)
        );

        During(AclRequested,
            When(AclRequestFailed)
                .Then(context =>
                {
                    context.Saga.Payload = context.Message.Payload;
                    context.Saga.ErrorMessage = context.Message.ErrorMessage;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    _logger.LogInformation($"ACL Process failed: {context.Saga.CorrelationId}");
                })
                .TransitionTo(AclFailed)
        );

        SetCompletedWhenFinalized();
    }
}