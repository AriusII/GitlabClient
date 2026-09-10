namespace GitLab.Client.Composition.WorkItems;

/// <summary>Describes whether one composition slice was requested and whether its DTO or GraphQL fragment is present.</summary>
public sealed record GitLabWorkItemCompositionSourceStatus
{
    /// <summary>Whether the composition plan selected this slice.</summary>
    public required bool WasRequested { get; init; }

    /// <summary>
    ///     Whether the matching DTO or GraphQL widget fragment was supplied. This remains independent of
    ///     <see cref="WasRequested" /> so callers can diagnose an accidental over-fetch or an incomplete plan.
    /// </summary>
    public required bool IsAvailable { get; init; }

    /// <summary>Whether the selected slice has a usable supplied value.</summary>
    public bool IsLoaded => WasRequested && IsAvailable;
}