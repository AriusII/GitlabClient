namespace GitLab.Client.Models;

/// <summary>
///     One external authentication provider linked to a GitLab account, as embedded in
///     <see cref="GitLabUser.Identities" />. Removed with
///     <c>DELETE /users/:id/identities/:provider</c>.
/// </summary>
public sealed record GitLabUserIdentity
{
    /// <summary>The provider name GitLab knows the identity by - <c>ldapmain</c>, <c>github</c>, <c>group_saml</c>.</summary>
    public required string Provider { get; init; }

    /// <summary>The user's identifier at the provider. Absent when the caller may not see it.</summary>
    public string? ExternUid { get; init; }

    /// <summary>For <c>group_saml</c> identities, the SAML provider the identity belongs to.</summary>
    public long? SamlProviderId { get; init; }
}