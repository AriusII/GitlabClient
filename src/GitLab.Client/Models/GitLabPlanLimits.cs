namespace GitLab.Client.Models;

/// <summary>
///     The resource limits configured for a billing plan, as returned by
///     <c>GET /application/plan_limits</c> and <c>PUT /application/plan_limits</c>. Administrators
///     only.
/// </summary>
public sealed record GitLabPlanLimits
{
    public long? CargoMaxFileSize { get; init; }

    public long? CiInstanceLevelVariables { get; init; }

    public long? CiPipelineSize { get; init; }

    public long? CiActiveJobs { get; init; }

    public long? CiProjectSubscriptions { get; init; }

    public long? CiPipelineSchedules { get; init; }

    public long? CiNeedsSizeLimit { get; init; }

    public long? CiRegisteredGroupRunners { get; init; }

    public long? CiRegisteredProjectRunners { get; init; }

    public long? ConanMaxFileSize { get; init; }

    public long? DotenvVariables { get; init; }

    public long? DotenvSize { get; init; }

    public long? EnforcementLimit { get; init; }

    public long? GenericPackagesMaxFileSize { get; init; }

    public long? HelmMaxFileSize { get; init; }

    /// <summary>
    ///     Recorded changes to the plan's limits, keyed by limit name. GitLab does not publish a fixed
    ///     schema for this map, so it is left open-ended rather than modeled per key.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<GitLabPlanLimitHistoryEntry>>? LimitsHistory { get; init; }

    public long? MavenMaxFileSize { get; init; }

    public long? NotificationLimit { get; init; }

    public long? NpmMaxFileSize { get; init; }

    public long? NugetMaxFileSize { get; init; }

    public long? PipelineHierarchySize { get; init; }

    public long? PypiMaxFileSize { get; init; }

    public long? ServiceDeskOutboundEmailsPerHour { get; init; }

    public long? ServiceDeskOutboundEmailsPerDay { get; init; }

    public long? TerraformModuleMaxFileSize { get; init; }

    public long? StorageSizeLimit { get; init; }

    public long? WebHookCalls { get; init; }

    public long? WebHookCallsLow { get; init; }

    public long? WebHookCallsMid { get; init; }

    public long? MaxPipelinesPerMergeTrain { get; init; }
}