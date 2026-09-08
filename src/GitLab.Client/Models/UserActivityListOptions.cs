using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for <c>GET /user/activities</c>.</summary>
[GitLabQuery]
public readonly record struct UserActivityListOptions
{
    /// <summary>
    ///     Return only users active on or after this day. GitLab defaults to six months back when omitted,
    ///     so a listing without this is not the whole history.
    /// </summary>
    public DateOnly? From { get; init; }

    public int? PerPage { get; init; }
}