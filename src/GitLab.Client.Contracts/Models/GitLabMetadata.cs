namespace GitLab.Client.Models;

/// <summary>
///     Version and edition metadata for this GitLab instance, as returned by <c>GET /metadata</c>.
/// </summary>
public sealed record GitLabMetadata
{
    /// <summary>The running GitLab version, for example <c>"18.4-pre"</c>.</summary>
    public string? Version { get; init; }

    /// <summary>The Git revision GitLab was built from.</summary>
    public string? Revision { get; init; }

    /// <summary>Status of the GitLab Agent Server for Kubernetes.</summary>
    public GitLabKasMetadata? Kas { get; init; }

    /// <summary>Whether this instance is running GitLab Enterprise Edition.</summary>
    public bool? Enterprise { get; init; }
}