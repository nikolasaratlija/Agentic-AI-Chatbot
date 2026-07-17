namespace Models;

public sealed record EHRAgentOutput
{
    public string? Response { get; init; }

    /// <summary>
    /// Whether the request is permitted to be processed.
    /// </summary>
    public bool Allowed { get; init; }

    /// <summary>
    /// The request classification determined before tool execution.
    /// </summary>
    public EHRAgentResponseClassification Classification { get; init; }

    /// <summary>
    /// Optional explanation when a request is denied.
    /// </summary>
    public string? RefusalReason { get; init; }
}

public enum EHRAgentResponseClassification
{
    Administrative,
    FactualRetrieval,

    // Denied
    ClinicalInterpretation,
    ClinicalDecisionSupport,

    // Contains both allowed and denied components
    Mixed,

    // Safety fallback
    Unknown
}