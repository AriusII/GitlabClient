namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /users/:user_id/personal_access_tokens</c>. Unlike
///     <see cref="CreateAccessTokenRequest" /> there is no access level: a personal access token always
///     acts with its owner's own permissions.
/// </summary>
public sealed record CreatePersonalAccessTokenRequest
{
    public required string Name { get; init; }

    /// <summary>The permissions of the token - for example <c>api</c>, <c>read_user</c>.</summary>
    public required IReadOnlyList<string> Scopes { get; init; }

    public string? Description { get; init; }

    /// <summary>
    ///     The expiry date. GitLab types this as a plain date here, unlike the date-time it returns on
    ///     <see cref="GitLabPersonalAccessToken.ExpiresAt" />.
    /// </summary>
    public DateOnly? ExpiresAt { get; init; }
}