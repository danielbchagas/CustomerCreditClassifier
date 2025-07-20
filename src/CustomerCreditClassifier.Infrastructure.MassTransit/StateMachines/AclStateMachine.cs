using CustomerCreditClassifier.Domain.Events.Acl;
using CustomerCreditClassifier.Domain.Events.NextService;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CustomerCreditClassifier.Infrastructure.MassTransit.StateMachines;

public class AclStateMachine : MassTransitStateMachine<SagaState>
{
    private readonly ILogger<AclStateMachine> _logger;
    
    public State Requested { get; private set; }
    public State Succeeded { get; private set; }
    public State Failed { get; private set; }
    
    public Event<AclRequested> RequestStarted { get; private set; }
    public Event<AclSucceeded> RequestSucceeded { get; private set; }
    public Event<AclFailed> RequestFailed { get; private set; }
    
    public AclStateMachine(ILogger<AclStateMachine> logger)
    {
        _logger = logger;
        
        InstanceState(x => x.CurrentState);

        Initially(
            When(RequestStarted)
                .Then(context =>
                {
                    context.Saga.Payload = context.Message.Payload;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    _logger.LogInformation($"ACL Process started for customer: {context.Saga.CorrelationId}");
                })
                .TransitionTo(Requested)
        );

        During(Requested,
            When(RequestSucceeded)
                .Then(context =>
                {
                    context.Saga.Payload = context.Message.Payload;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    _logger.LogInformation($"ACL Process succeeded for customer: {context.Saga.CorrelationId}");
                })
                .Publish(context => new NextServiceRequested
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
                    _logger.LogInformation($"ACL Process failed: {context.Saga.CorrelationId}");
                })
                .TransitionTo(Failed)
        );

        SetCompletedWhenFinalized();
    }
}