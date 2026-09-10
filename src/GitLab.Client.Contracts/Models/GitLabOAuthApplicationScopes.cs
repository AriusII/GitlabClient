using GitLab.Client.Models.Requests;

namespace GitLab.Client.Models;

/// <summary>
///     Well-known values for an OAuth application's <c>scopes</c> - the space-separated string on
///     <see cref="CreateApplicationRequest.Scopes" />/<see cref="UpdateApplicationRequest.Scopes" />
///     and the array GitLab echoes back on <see cref="GitLabApplication.Scopes" />.
///     <para>
///         These are constants rather than an enum on purpose: GitLab adds OAuth scopes in minor
///         releases, and a closed enum on <see cref="GitLabApplication.Scopes" /> would turn the
///         arrival of a new scope into a hard deserialization failure on an otherwise healthy listing.
///     </para>
/// </summary>
public static class GitLabOAuthApplicationScopes
{
    /// <summary>Full read/write access to the API, including all groups and projects.</summary>
    public const string Api = "api";

    /// <summary>Read-only access to the API.</summary>
    public const string ReadApi = "read_api";

    /// <summary>Read-only access to the authenticated user's profile.</summary>
    public const string ReadUser = "read_user";

    /// <summary>Read-only access to a private token's repositories.</summary>
    public const string ReadRepository = "read_repository";

    /// <summary>Read/write access to a private token's repositories.</summary>
    public const string WriteRepository = "write_repository";

    /// <summary>Read-only access to container registry images.</summary>
    public const string ReadRegistry = "read_registry";

    /// <summary>Read/write access to container registry images.</summary>
    public const string WriteRegistry = "write_registry";

    /// <summary>Grants access to the OpenID Connect <c>userinfo</c> claims.</summary>
    public const string OpenId = "openid";

    /// <summary>Grants read-only access to the user's primary email address via OpenID Connect.</summary>
    public const string Profile = "profile";

    /// <summary>Grants access to the OpenID Connect email claims.</summary>
    public const string Email = "email";

    /// <summary>Grants unrestricted access to the instance, acting as any user - administrators only.</summary>
    public const string Sudo = "sudo";
}