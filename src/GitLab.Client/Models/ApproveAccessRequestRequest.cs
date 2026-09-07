namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /projects/:id/access_requests/:user_id/approve</c> and its group
///     counterpart. Every member is optional: approving with no body applies GitLab's default of
///     <c>30</c> (the Developer role).
/// </summary>
public sealed record ApproveAccessRequestRequest
{
    /// <summary>The access level to grant - 10 Guest, 20 Reporter, 30 Developer, 40 Maintainer, 50 Owner. Defaults to 30.</summary>
    public int? AccessLevel { get; init; }
}