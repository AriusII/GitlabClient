namespace GitLab.Client.Models;

/// <summary>
///     A deployment environment within a GitLab project, as returned by the Environments API.
///     <para>
///         The <c>project</c>, <c>last_deployment</c> and <c>cluster_agent</c> sub-entities the API also
///         returns are deliberately not modelled here - they would drag in project, deployment and cluster
///         agent DTOs that no resource in this library owns yet.
///     </para>
/// </summary>
public sealed record GitLabEnvironment
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public string? Slug { get; init; }

    public Uri? ExternalUrl { get; init; }

    /// <summary>
    ///     "available", "stopping" or "stopped". Nullable because GitLab embeds the *basic* environment
    ///     shape (<c>APIEntitiesEnvironmentBasic</c>, which has no <c>state</c>) inside a deployment.
    /// </summary>
    public string? State { get; init; }

    /// <summary>The deployment tier: <c>production</c>, <c>staging</c>, <c>testing</c>, <c>development</c> or <c>other</c>.</summary>
    public string? Tier { get; init; }

    /// <summary>When the environment is scheduled to stop automatically, if a stop is scheduled at all.</summary>
    public DateTimeOffset? AutoStopAt { get; init; }

    /// <summary>How auto-stop is triggered - <c>always</c> or <c>with_action</c>. A plain string, not a flag.</summary>
    public string? AutoStopSetting { get; init; }

    public string? Description { get; init; }

    public string? KubernetesNamespace { get; init; }

    public string? FluxResourcePath { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}