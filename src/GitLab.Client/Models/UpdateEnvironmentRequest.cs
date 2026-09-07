namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /projects/:id/environments/:environment_id</c>. Every member is optional:
///     unset ones are omitted from the payload rather than sent as null, so a partial update cannot clear a
///     field it never mentioned. <c>name</c> is absent because the update schema does not accept one.
/// </summary>
public sealed record UpdateEnvironmentRequest
{
    public Uri? ExternalUrl { get; init; }

    /// <summary><c>production</c>, <c>staging</c>, <c>testing</c>, <c>development</c> or <c>other</c>.</summary>
    public string? Tier { get; init; }

    public long? ClusterAgentId { get; init; }

    public string? KubernetesNamespace { get; init; }

    public string? FluxResourcePath { get; init; }

    public string? Description { get; init; }

    /// <summary><c>always</c> or <c>with_action</c>.</summary>
    public string? AutoStopSetting { get; init; }
}