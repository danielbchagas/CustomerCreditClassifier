using CustomerCreditClassifier.Domain.Events;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerCreditClassifier.Infrastructure.MassTransit.Mappings;

public class SagaEventMap : SagaClassMap<SagaEvent>
{
    protected override void Configure(EntityTypeBuilder<SagaEvent> entity, ModelBuilder model)
    {
        entity.HasKey(x => x.CorrelationId);
        entity.Property(x => x.CurrentState).HasColumnType("VARCHAR").HasMaxLength(64);
        entity.Property(x => x.Payload).HasColumnType("VARCHAR").HasMaxLength(1024).HasJsonConversion();
        entity.Property(x => x.CreatedAt).HasColumnType("DATE");
    }
}