using System.Text.Json.Nodes;
using MassTransit;

namespace CustomerCreditClassifier.Domain.Events;

public class SagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string? CurrentState { get; set; }
    public int Version { get; set; }
    public required JsonObject Payload { get; set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }
    public string? ErrorMessage { get; set; }
}