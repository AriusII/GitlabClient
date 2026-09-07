namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /user/applications/:id</c>. Every member is nullable: unset properties
///     are omitted from the payload rather than sent as null, so an update carries only what actually
///     changes and leaves the rest of the application's configuration alone.
/// </summary>
public sealed record UpdateApplicationRequest
{
    /// <summary>The replacement display name for the application.</summary>
    public string? Name { get; init; }

    /// <summary>
    ///     The replacement space-separated OAuth scopes. See <see cref="GitLabOAuthApplicationScopes" />
    ///     for the well-known values.
    /// </summary>
    public string? Scopes { get; init; }
}