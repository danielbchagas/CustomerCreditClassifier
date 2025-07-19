using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CustomerCreditClassifier.Infrastructure.MassTransit;

public class SagaDbContext : DbContext
{
    public SagaDbContext(DbContextOptions<SagaDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Adiciona os mapeamentos para a saga e o outbox
        // modelBuilder.AddSagaStateMachines(typeof(YourStateMachine).Assembly);
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}