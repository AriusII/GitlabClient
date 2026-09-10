namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PATCH /groups/:id/saml/:uid</c> and <c>PATCH /groups/:id/scim/:uid</c> -
///     re-pointing an existing identity at a new external UID, which is what has to happen when the
///     identity provider re-keys a user.
/// </summary>
public sealed record UpdateProviderIdentityRequest
{
    /// <summary>The new external UID for the identity. The old one is addressed in the route.</summary>
    public required string ExternUid { get; init; }
}