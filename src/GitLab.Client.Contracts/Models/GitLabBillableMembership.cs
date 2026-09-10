namespace GitLab.Client.Models;

/// <summary>
///     One membership held by a billable member of a top-level group, as returned by
///     <c>GET /groups/:id/billable_members/:user_id/memberships</c> and its <c>/indirect</c> counterpart.
///     <para>
///         A billable member usually holds several of these: the source is the group or project that
///         actually carries the membership, which is why <see cref="SourceFullName" /> and
///         <see cref="SourceMembersUrl" /> matter more here than the membership's own ID.
///     </para>
/// </summary>
public sealed record GitLabBillableMembership
{
    /// <summary>The membership's own ID - not the user's. Typed as a string in the spec; GitLab sends a number.</summary>
    public long? Id { get; init; }

    /// <summary>ID of the group or project the membership is held on.</summary>
    public long? SourceId { get; init; }

    /// <summary>Full path of the group or project the membership is held on, for example "Group / Subgroup".</summary>
    public string? SourceFullName { get; init; }

    /// <summary>Link to the source's members page.</summary>
    public Uri? SourceMembersUrl { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>When the membership lapses. A plain date, not a timestamp.</summary>
    public DateOnly? ExpiresAt { get; init; }

    public GitLabBillableMembershipAccessLevel? AccessLevel { get; init; }
}