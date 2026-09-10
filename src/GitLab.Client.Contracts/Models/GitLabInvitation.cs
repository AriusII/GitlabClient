namespace GitLab.Client.Models;

/// <summary>
///     A pending invitation to a project or group, as returned by the GitLab Invitations API
///     (<c>/projects/:id/invitations</c>, <c>/groups/:id/invitations</c>).
///     <para>
///         Every member is deliberately optional. The create endpoint invites many people in one call
///         and answers with a status envelope (<c>{"status":"success"}</c>, plus a <c>message</c> map
///         when some invitations failed) rather than the single invitation entity the spec advertises,
///         so an all-null instance is the correct outcome there instead of a deserialization failure.
///     </para>
/// </summary>
public sealed record GitLabInvitation
{
    public long? Id { get; init; }

    /// <summary>
    ///     The invited access level - 10 Guest, 20 Reporter, 30 Developer, 40 Maintainer, 50 Owner (and 5,
    ///     Minimal Access, on create). Typed <c>string</c> in the spec, but GitLab sends a number.
    /// </summary>
    public int? AccessLevel { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? ExpiresAt { get; init; }

    public string? InviteEmail { get; init; }

    public string? InviteToken { get; init; }

    public string? UserName { get; init; }

    public string? CreatedByName { get; init; }
}