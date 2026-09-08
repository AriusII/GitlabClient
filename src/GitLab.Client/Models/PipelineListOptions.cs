using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for listing a project's pipelines (<c>GET /projects/:id/pipelines</c>).</summary>
[GitLabQuery]
public sealed record PipelineListOptions
{
    public string? Status { get; init; }

    public string? Ref { get; init; }

    public string? Sha { get; init; }

    public string? Source { get; init; }

    /// <summary>Filters to pipelines whose name matches exactly.</summary>
    public string? Name { get; init; }

    /// <summary>The username of the user who triggered the pipeline.</summary>
    public string? Username { get; init; }

    /// <summary>"running", "pending", "finished", "branches" or "tags".</summary>
    public string? Scope { get; init; }

    public DateTimeOffset? UpdatedAfter { get; init; }

    public DateTimeOffset? UpdatedBefore { get; init; }

    /// <summary>Returns only pipelines created strictly after this instant.</summary>
    public DateTimeOffset? CreatedAfter { get; init; }

    /// <summary>Returns only pipelines created strictly before this instant.</summary>
    public DateTimeOffset? CreatedBefore { get; init; }

    public bool? YamlErrors { get; init; }

    public string? OrderBy { get; init; }

    public string? Sort { get; init; }

    public int? PerPage { get; init; }
}