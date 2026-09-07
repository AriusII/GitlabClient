namespace GitLab.Client.Models;

/// <summary>
///     The state of a repository's commit-graph cache, from
///     <c>GET /projects/:id/repository/health</c>.
/// </summary>
public sealed record GitLabRepositoryHealthCommitGraph
{
    /// <summary>How many files the commit-graph chain is split across.</summary>
    public long? CommitGraphChainLength { get; init; }

    public bool? HasBloomFilters { get; init; }

    public bool? HasGenerationData { get; init; }

    public bool? HasGenerationDataOverflow { get; init; }
}