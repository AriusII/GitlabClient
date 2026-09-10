namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>POST /projects/:id/environments</c>.</summary>
public sealed record CreateEnvironmentRequest
{
    public required string Name { get; init; }

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