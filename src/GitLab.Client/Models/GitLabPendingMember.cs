namespace GitLab.Client.Models;

/// <summary>
///     One entry of <c>GET /groups/:id/pending_members</c> - a member in the <c>awaiting</c> state, or
///     somebody invited by email who has no GitLab account yet.
///     <para>
///         Every member is optional on purpose. The spec documents no response schema for this endpoint,
///         and an invitation that has not been redeemed carries no user, so <see cref="Id" />,
///         <see cref="Username" /> and <see cref="WebUrl" /> are absent for those rows while
///         <see cref="Email" /> is the only identifier present.
///     </para>
/// </summary>
public sealed record GitLabPendingMember
{
    public long? Id { get; init; }

    public string? Name { get; init; }

    public string? Username { get; init; }

    /// <summary>The invited email address. The only identifier present for an invitee with no GitLab account.</summary>
    public string? Email { get; init; }

    public Uri? AvatarUrl { get; init; }

    public Uri? WebUrl { get; init; }

    /// <summary>Whether the membership has already been approved.</summary>
    public bool? Approved { get; init; }

    /// <summary>Whether this row came from an email invitation rather than an access request.</summary>
    public bool? Invited { get; init; }
}