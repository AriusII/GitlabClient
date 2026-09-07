using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the GitLab "Provider identities" API area (<c>/groups/:id/saml/...</c> and
///     <c>/groups/:id/scim/...</c>) - the links between a GitLab user and its identity at a group's
///     SAML or SCIM provider.
///     <para>
///         The two provider families are separate stores with an identical payload shape, which is why
///         the methods pair up: a user signing in through group SAML gets a SAML identity, and the same
///         user provisioned by the SCIM connector gets a SCIM identity. Re-keying one does not re-key
///         the other, and the usual reason to call the update methods is exactly that - the identity
///         provider changed the user's external UID and both sides have to be pointed at the new one.
///     </para>
///     <para>
///         Group SAML and SCIM are Premium/Ultimate features on a top-level group; on an instance or
///         plan without them GitLab answers <c>404</c>.
///     </para>
/// </summary>
public interface IProviderIdentitiesClient
{
    /// <summary>Streams every SAML identity in a group.</summary>
    IAsyncEnumerable<GitLabProviderIdentity> ListSamlAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one SAML identity by its external UID. The UID is opaque provider-supplied text and may
    ///     contain <c>/</c>, <c>@</c> or spaces; pass it raw, the route builder percent-encodes it.
    /// </summary>
    Task<GitLabProviderIdentity> GetSamlAsync(GroupId groupId, string externUid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Re-points a SAML identity at a new external UID. <paramref name="externUid" /> is the
    ///     <em>current</em> UID; the new one goes in the request body.
    /// </summary>
    Task<GitLabProviderIdentity> UpdateSamlAsync(GroupId groupId, string externUid,
        UpdateProviderIdentityRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a SAML identity, unlinking the user from the group's SAML provider. The GitLab user
    ///     and its group membership are left alone.
    /// </summary>
    Task DeleteSamlAsync(GroupId groupId, string externUid, CancellationToken cancellationToken = default);

    /// <summary>Streams every SCIM identity in a group.</summary>
    IAsyncEnumerable<GitLabProviderIdentity> ListScimAsync(GroupId groupId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets one SCIM identity by its external UID. The UID is opaque provider-supplied text; pass it
    ///     raw, the route builder percent-encodes it.
    /// </summary>
    Task<GitLabProviderIdentity> GetScimAsync(GroupId groupId, string externUid,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Re-points a SCIM identity at a new external UID. <paramref name="externUid" /> is the
    ///     <em>current</em> UID; the new one goes in the request body.
    /// </summary>
    Task<GitLabProviderIdentity> UpdateScimAsync(GroupId groupId, string externUid,
        UpdateProviderIdentityRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a SCIM identity. Depending on the group's settings this can also block the linked
    ///     user, so treat it as more than an unlink.
    /// </summary>
    Task DeleteScimAsync(GroupId groupId, string externUid, CancellationToken cancellationToken = default);
}