using CustomerCreditClassifier.Infrastructure.MassTransit.Mappings;
using CustomerCreditClassifier.Infrastructure.MassTransit.StateMachines;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace CustomerCreditClassifier.Infrastructure.MassTransit.Data;

public class StateMachineDbContext : SagaDbContext
{
    public StateMachineDbContext(DbContextOptions<StateMachineDbContext> options) : base(options)
    {
    }
    
    public DbSet<SagaState> States { get; set; }

    protected override IEnumerable<ISagaClassMap> Configurations
    {
        get { yield return new SagaStateMap(); }
    }
}