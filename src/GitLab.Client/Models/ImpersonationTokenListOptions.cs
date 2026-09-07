using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for listing a user's impersonation tokens
///     (<c>GET /users/:user_id/impersonation_tokens</c>). A narrower filter set than
///     <see cref="PersonalAccessTokenListOptions" /> - GitLab accepts only a state filter here.
/// </summary>
[GitLabQuery]
public sealed record ImpersonationTokenListOptions
{
    /// <summary><c>all</c>, <c>active</c> or <c>inactive</c>. Defaults to <c>all</c>.</summary>
    public string? State { get; init; }

    public int? PerPage { get; init; }
}