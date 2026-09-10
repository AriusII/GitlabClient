namespace GitLab.Client.Models.Requests;

/// <summary>Request body for the administrator-only <c>POST /deploy_keys</c> endpoint.</summary>
/// <remarks>
///     A deployment key created at instance scope has no project-specific push permission. Attach it to a project
///     with the deploy-key client after creation.
/// </remarks>
public sealed record CreateInstanceDeployKeyRequest
{
    /// <summary>The public key material, in <c>authorized_keys</c> form.</summary>
    public required string Key { get; init; }

    public required string Title { get; init; }

    /// <summary>An optional expiry, sent as an ISO 8601 timestamp.</summary>
    public DateTimeOffset? ExpiresAt { get; init; }
}