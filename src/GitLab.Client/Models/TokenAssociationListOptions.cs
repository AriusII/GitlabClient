using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for <c>GET /personal_access_tokens/self/associations</c>.
///     <para>
///         Unlike every other list-options record in this library this one carries a
///         <see cref="Page" />, because the endpoint answers with a single
///         <c>{ "groups": [...], "projects": [...] }</c> object rather than a JSON array - so it is
///         fetched with a plain GET and cannot be walked by the <c>Link: rel="next"</c> streaming that
///         makes an explicit page number unnecessary elsewhere.
///     </para>
/// </summary>
[GitLabQuery]
public sealed record TokenAssociationListOptions
{
    /// <summary>
    ///     Return only groups and projects where the token owner has at least this role: 10 (Guest),
    ///     15 (Planner), 20 (Reporter), 25, 30 (Developer), 40 (Maintainer) or 50 (Owner).
    /// </summary>
    public int? MinAccessLevel { get; init; }

    public int? Page { get; init; }

    public int? PerPage { get; init; }
}