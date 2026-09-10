namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /user/personal_access_tokens</c>, which creates a personal access token
///     for the currently authenticated user.
///     <para>
///         Unlike <see cref="CreatePersonalAccessTokenRequest" /> (the administrator route for another
///         user) only the name is mandatory here, because GitLab deliberately restricts what this route
///         may grant: the accepted scopes are <see cref="GitLabTokenScopes.K8sProxy" /> and
///         <see cref="GitLabTokenScopes.SelfRotate" />. Anything wider must go through the UI or an
///         administrator route.
///     </para>
/// </summary>
public sealed record CreateCurrentUserPersonalAccessTokenRequest
{
    public required string Name { get; init; }

    /// <summary>
    ///     The permissions of the token. Limited to <c>k8s_proxy</c> and <c>self_rotate</c> on this
    ///     route. Mutually exclusive with <see cref="GranularScopes" />.
    /// </summary>
    public IReadOnlyList<string>? Scopes { get; init; }

    /// <summary>
    ///     Granular permissions to assign instead of the coarse <see cref="Scopes" />. Mutually
    ///     exclusive with <see cref="Scopes" />.
    /// </summary>
    public IReadOnlyList<GitLabGranularScope>? GranularScopes { get; init; }

    public string? Description { get; init; }

    /// <summary>
    ///     The expiry date. GitLab types this as a plain date, not a date-time. When omitted the token
    ///     expires at the instance's maximum allowable lifetime.
    /// </summary>
    public DateOnly? ExpiresAt { get; init; }
}