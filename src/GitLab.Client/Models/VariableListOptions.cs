using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Paging for the CI/CD variable list endpoints (<c>GET /projects/:id/variables</c> and
///     <c>GET /groups/:id/variables</c>). Neither endpoint takes a filter - <c>page</c> and
///     <c>per_page</c> are the only query parameters in the spec.
/// </summary>
[GitLabQuery]
public readonly record struct VariableListOptions
{
    public int? PerPage { get; init; }
}