namespace GitLab.Client.Models;

/// <summary>
///     A cached variant assignment for one experiment context - the answer to
///     <c>GET/POST /experiments/:experiment_name/assignments</c>.
/// </summary>
public sealed record GitLabExperimentAssignment
{
    public string? Experiment { get; init; }

    /// <summary>The assigned variant name - "control", "candidate", or whatever the experiment defines.</summary>
    public string? Variant { get; init; }

    /// <summary>The cache key GitLab derived from the context that was supplied (or the current user, by default).</summary>
    public string? ContextKey { get; init; }

    /// <summary>Whether this assignment came from Redis rather than being computed fresh.</summary>
    public bool? Cached { get; init; }
}