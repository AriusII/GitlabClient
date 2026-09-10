namespace GitLab.Client.Models;

/// <summary>
///     One project or group a user is a member of, as returned by
///     <c>GET /users/:id/memberships</c>. This is the administrator's cross-instance view of a single
///     account's memberships, and is a different, flatter shape from <see cref="GitLabMember" />, which
///     is the per-project/per-group view of who belongs to one source.
/// </summary>
public sealed record GitLabUserMembership
{
    /// <summary>The ID of the project or group the membership is in.</summary>
    public required long SourceId { get; init; }

    public required string SourceName { get; init; }

    /// <summary>
    ///     <c>Project</c> or <c>Namespace</c>. Kept as a string on the response even though the matching
    ///     <see cref="UserMembershipListOptions.Type" /> filter is an enum: the spec enumerates the filter's
    ///     vocabulary but not this field's, and an unrecognised source type must not fail the whole listing.
    /// </summary>
    public required string SourceType { get; init; }

    /// <summary>
    ///     The role on the numeric ladder - 10 Guest, 20 Reporter, 30 Developer, 40 Maintainer, 50 Owner,
    ///     plus the 5 (Minimal Access), 15 (Planner) and 25 (Admin) rungs. Typed as a string in the spec,
    ///     but GitLab sends an integer.
    /// </summary>
    public required int AccessLevel { get; init; }
}