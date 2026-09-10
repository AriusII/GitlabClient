namespace GitLab.Client.Models;

/// <summary>
///     One SAML or SCIM identity linking a GitLab user to an external identity provider, as returned
///     by the group SAML and SCIM identity endpoints (<c>/groups/:id/saml/identities</c>,
///     <c>/groups/:id/saml/:uid</c>, <c>/groups/:id/scim/identities</c>, <c>/groups/:id/scim/:uid</c>)
///     and embedded as <c>group_scim_identity</c> in a member response.
///     <para>
///         Both provider families answer with the same shape, so one type serves both. The
///         distinction lives in the call, not in the payload: a SAML identity is created by the user
///         signing in through the group's SAML provider, a SCIM identity by the provider provisioning
///         the user.
///     </para>
///     <para>
///         Not to be confused with <see cref="GitLabUserIdentity" />, which is the provider list
///         embedded in a user record and is keyed by provider name rather than by external UID.
///     </para>
/// </summary>
public sealed record GitLabProviderIdentity
{
    /// <summary>
    ///     The user's identifier at the provider - the SAML <c>NameID</c> or the SCIM <c>externalId</c>.
    ///     Opaque caller-supplied text: it may contain slashes, spaces and <c>@</c>, which is why every
    ///     route that takes it percent-encodes it.
    /// </summary>
    public required string ExternUid { get; init; }

    /// <summary>The ID of the GitLab user the identity resolves to.</summary>
    public long? UserId { get; init; }

    /// <summary>
    ///     The group that owns an embedded <c>group_scim_identity</c>. The standalone identity routes
    ///     are already scoped by their route group and generally omit this field.
    /// </summary>
    public long? GroupId { get; init; }

    /// <summary>
    ///     Whether the identity is currently active. SCIM identities carry this; the SAML endpoints
    ///     generally omit it, hence nullable.
    /// </summary>
    public bool? Active { get; init; }
}