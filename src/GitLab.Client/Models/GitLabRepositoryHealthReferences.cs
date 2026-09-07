namespace GitLab.Client.Models;

/// <summary>How a repository stores its refs, from <c>GET /projects/:id/repository/health</c>.</summary>
public sealed record GitLabRepositoryHealthReferences
{
    /// <summary>Refs still stored as individual loose files rather than in the packed-refs file.</summary>
    public long? LooseCount { get; init; }

    /// <summary>Size in bytes of the packed-refs file.</summary>
    public long? PackedSize { get; init; }

    /// <summary>The Git reference backend in use, for example <c>files</c> or <c>reftable</c>.</summary>
    public string? ReferenceBackend { get; init; }
}