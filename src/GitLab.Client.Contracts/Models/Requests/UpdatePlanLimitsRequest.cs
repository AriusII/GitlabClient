namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PUT /application/plan_limits</c>. Every limit is nullable: unset properties
///     are omitted from the payload rather than sent as null, so an update leaves limits it does not
///     mention unchanged. Administrators only.
/// </summary>
public sealed record UpdatePlanLimitsRequest
{
    /// <summary>The plan whose limits are being changed.</summary>
    public required GitLabPlanName PlanName { get; init; }

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

    public long? DotenvSize { get; init; }

    public long? DotenvVariables { get; init; }

    public long? EnforcementLimit { get; init; }

    public long? GenericPackagesMaxFileSize { get; init; }

    public long? HelmMaxFileSize { get; init; }

    public long? MavenMaxFileSize { get; init; }

    public long? NotificationLimit { get; init; }

    public long? NpmMaxFileSize { get; init; }

    public long? NugetMaxFileSize { get; init; }

    public long? PypiMaxFileSize { get; init; }

    public long? ServiceDeskOutboundEmailsPerHour { get; init; }

    public long? ServiceDeskOutboundEmailsPerDay { get; init; }

    public long? TerraformModuleMaxFileSize { get; init; }

    public long? StorageSizeLimit { get; init; }

    public long? PipelineHierarchySize { get; init; }

    public long? WebHookCalls { get; init; }

    public long? WebHookCallsLow { get; init; }

    public long? WebHookCallsMid { get; init; }

    public long? MaxPipelinesPerMergeTrain { get; init; }
}