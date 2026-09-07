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

    public DateTimeOffset? UpdatedAfter { get; init; }

    public DateTimeOffset? UpdatedBefore { get; init; }

    public bool? YamlErrors { get; init; }

    public string? OrderBy { get; init; }

    public string? Sort { get; init; }

    public int? PerPage { get; init; }
}