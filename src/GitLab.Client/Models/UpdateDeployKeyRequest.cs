namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /projects/:id/deploy_keys/:key_id</c>. The title and the push flag are the only
///     two fields the endpoint accepts; the key material itself is immutable.
/// </summary>
public sealed record UpdateDeployKeyRequest
{
    public string? Title { get; init; }

    public bool? CanPush { get; init; }
}