namespace Models;

public sealed record BiGatekeeperDecision
{
    public required bool IsClear { get; init; }

    public string? ClarificationQuestion { get; init; }

    public SemanticIntent? Intent { get; init; }

    public required string ReasoningSummary { get; init; }
}