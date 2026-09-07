namespace GitLab.Client.Models;

/// <summary>
///     One user's most recent activity, as returned by <c>GET /user/activities</c>. Requires instance
///     administrator rights: despite the <c>/user</c> prefix the endpoint reports on every user on the
///     instance, not on the token's owner.
/// </summary>
public sealed record GitLabUserActivity
{
    public required string Username { get; init; }

    /// <summary>
    ///     The day the user was last active. GitLab tracks activity at day granularity, which is why this is
    ///     a date and not a timestamp - the spec types it as a bare <c>string</c>, but the wire value is
    ///     <c>YYYY-MM-DD</c>.
    /// </summary>
    public DateOnly? LastActivityOn { get; init; }

    /// <summary>
    ///     The same day as <see cref="LastActivityOn" />, under GitLab's older field name. Kept because the
    ///     endpoint still sends both; prefer <see cref="LastActivityOn" />.
    /// </summary>
    public DateOnly? LastActivityAt { get; init; }
}