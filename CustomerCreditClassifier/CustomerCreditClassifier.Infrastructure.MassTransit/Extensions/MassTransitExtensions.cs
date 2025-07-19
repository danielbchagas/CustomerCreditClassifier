using CustomerCreditClassifier.Domain.Events;
using CustomerCreditClassifier.Domain.Events.AntiCorruptionLayer;
using CustomerCreditClassifier.Domain.Events.NextService;
using CustomerCreditClassifier.Domain.Events.PreviousService;
using CustomerCreditClassifier.Infrastructure.MassTransit.StateMachines;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerCreditClassifier.Infrastructure.MassTransit.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<SagaDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.AddSagaStateMachine<PreviousServiceStateMachine, SagaStateBase>()
                .EntityFrameworkRepository(r =>
                {
                    r.AddDbContext<DbContext, SagaDbContext>();
                    r.UseSqlServer();
                    r.LockStatementProvider = new SqlServerLockStatementProvider();
                });
            
            x.AddSagaStateMachine<AclStateMachine, SagaStateBase>()
                .EntityFrameworkRepository(r =>
                {
                    r.AddDbContext<DbContext, SagaDbContext>();
                    r.UseSqlServer();
                    r.LockStatementProvider = new SqlServerLockStatementProvider();
                });
            
            x.AddSagaStateMachine<NextServiceStateMachine, SagaStateBase>()
                .EntityFrameworkRepository(r =>
                {
                    r.AddDbContext<DbContext, SagaDbContext>();
                    r.UseSqlServer();
                    r.LockStatementProvider = new SqlServerLockStatementProvider();
                });
            
            x.AddEntityFrameworkOutbox<SagaDbContext>(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(10);
                o.UseSqlServer();
                o.UseBusOutbox();
            });

            x.AddRider(rider =>
            {
                rider.UsingKafka((context, k) =>
                {
                    k.Host(configuration.GetConnectionString("Kafka"));

                    PreviousServiceTopicEndpoint(k, context);
                    AclTopicEndpoint(k, context);
                    NextServiceTopicProducer(k, context);
                });
            });
        });

        return services;
    }

    private static void PreviousServiceTopicEndpoint(IKafkaFactoryConfigurator k, IRiderRegistrationContext context)
    {
        k.TopicEndpoint<PreviousServiceRequested>("previous-customer-credit-classification-requested", "classifier-state-machine-group", e =>
        {
            e.ConfigureSaga<SagaStateBase>(context);
        });
                    
        k.TopicEndpoint<PreviousServiceSucceeded>("previous-customer-credit-classification-succeeded", "classifier-state-machine-group", e =>
        {
            e.ConfigureSaga<SagaStateBase>(context);
        });
                    
        k.TopicEndpoint<PreviousServiceFailed>("previous-customer-credit-classification-failed", "classifier-state-machine-group", e =>
        {
            e.ConfigureSaga<SagaStateBase>(context);
        });
    }

    private static void AclTopicEndpoint(IKafkaFactoryConfigurator k, IRiderRegistrationContext context)
    {
        k.TopicEndpoint<AclRequested>("acl-customer-credit-classification-requested", "classifier-state-machine-group", e =>
        {
            e.ConfigureSaga<SagaStateBase>(context);
        });
                    
        k.TopicEndpoint<AclSucceeded>("acl-customer-credit-classification-succeeded", "classifier-state-machine-group", e =>
        {
            e.ConfigureSaga<SagaStateBase>(context);
        });
                    
        k.TopicEndpoint<AclFailed>("acl-customer-credit-classification-failed", "classifier-state-machine-group", e =>
        {
            e.ConfigureSaga<SagaStateBase>(context);
        });
    }
    
    private static void NextServiceTopicProducer(IKafkaFactoryConfigurator k, IRiderRegistrationContext context)
    {
        k.TopicEndpoint<NextServiceRequested>("next-customer-credit-classification-requested", "classifier-state-machine-group", e =>
        {
            e.ConfigureSaga<SagaStateBase>(context);
        });
                    
        k.TopicEndpoint<NextServiceSucceeded>("next-customer-credit-classification-succeeded", "classifier-state-machine-group", e =>
        {
            e.ConfigureSaga<SagaStateBase>(context);
        });
                    
        k.TopicEndpoint<NextServiceFailed>("next-customer-credit-classification-failed", "classifier-state-machine-group", e =>
        {
            e.ConfigureSaga<SagaStateBase>(context);
        });
    }
}