using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Paging for listing a release's asset links
///     (<c>GET /projects/:id/releases/:tag_name/assets/links</c>). GitLab declares no filters on this
///     endpoint beyond the page size.
/// </summary>
[GitLabQuery]
public readonly record struct ReleaseLinkListOptions
{
    public int? PerPage { get; init; }
}