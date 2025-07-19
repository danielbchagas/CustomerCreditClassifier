using CustomerCreditClassifier.Domain.Events;
using CustomerCreditClassifier.Infrastructure.MassTransit.Mappings;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace CustomerCreditClassifier.Infrastructure.MassTransit;

public class StateMachineDbContext : SagaDbContext
{
    public StateMachineDbContext(DbContextOptions<StateMachineDbContext> options) : base(options)
    {
    }
    
    public DbSet<SagaStateBase> States { get; set; }

    protected override IEnumerable<ISagaClassMap> Configurations
    {
        get { yield return new SagaEventMap(); }
    }
}