using System.Reflection;
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

        services.AddDbContext<SagaDbContext, StateMachineDbContext>(opt =>
        {
            opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), options =>
            {
                options.EnableRetryOnFailure();
            });
        });

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.AddSagaStateMachine<PreviousServiceStateMachine, SagaStateBase>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                    
                    r.AddDbContext<SagaDbContext, StateMachineDbContext>((provider,builder) =>
                    {
                        builder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), options =>
                        {
                            options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                            options.MigrationsHistoryTable($"__{nameof(StateMachineDbContext)}");
                            options.EnableRetryOnFailure();
                        });
                    });
                    
                    r.UsePostgres();
                });
            
            x.AddSagaStateMachine<AclStateMachine, SagaStateBase>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                    
                    r.AddDbContext<SagaDbContext, StateMachineDbContext>((provider,builder) =>
                    {
                        builder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), options =>
                        {
                            options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                            options.MigrationsHistoryTable($"__{nameof(StateMachineDbContext)}");
                            options.EnableRetryOnFailure();
                        });
                    });
                    
                    r.UsePostgres();
                });
            
            x.AddSagaStateMachine<NextServiceStateMachine, SagaStateBase>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                    
                    r.AddDbContext<SagaDbContext, StateMachineDbContext>((provider,builder) =>
                    {
                        builder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), options =>
                        {
                            options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                            options.MigrationsHistoryTable($"__{nameof(StateMachineDbContext)}");
                            options.EnableRetryOnFailure();
                        });
                    });
                    
                    r.UsePostgres();
                });

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
            
            x.AddRider(rider =>
            {
                rider.AddSagaStateMachine<PreviousServiceStateMachine, SagaStateBase>();
                rider.AddSagaStateMachine<AclStateMachine, SagaStateBase>();
                rider.AddSagaStateMachine<NextServiceStateMachine, SagaStateBase>();
                
                rider.UsingKafka((context, k) =>
                {
                    k.Host(configuration.GetSection("Kafka:BootstrapServers").Value);

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