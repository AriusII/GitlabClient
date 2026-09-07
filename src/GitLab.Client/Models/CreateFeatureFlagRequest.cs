namespace GitLab.Client.Models;

/// <summary>Body of <c>POST /projects/:id/feature_flags</c>.</summary>
public sealed record CreateFeatureFlagRequest
{
    /// <summary>The name of the new flag, which becomes its route key.</summary>
    public required string Name { get; init; }

    public string? Description { get; init; }

    /// <summary>Whether the flag is enabled. GitLab defaults it to <c>true</c>.</summary>
    public bool? Active { get; init; }

    /// <summary>The strategies to create the flag with.</summary>
    public IReadOnlyList<FeatureFlagStrategyRequest>? Strategies { get; init; }
}