namespace Models;

public sealed record SemanticIntent
{
    public required string UserGoal { get; init; }

    public required string Metric { get; init; }

    public string? Entity { get; init; }

    public string? Dimension { get; init; }

    public string? TimeRange { get; init; }

    public string? Filter { get; init; }

    public int? Limit { get; init; }

    public string? VisualizationHint { get; init; }

    public IReadOnlyList<string> Assumptions { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> RequiredTables { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> RequiredColumns { get; init; } = Array.Empty<string>();
}