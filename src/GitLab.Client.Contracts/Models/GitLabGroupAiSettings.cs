namespace GitLab.Client.Models;

/// <summary>AI settings embedded by the detailed group response.</summary>
public sealed record GitLabGroupAiSettings
{
    public bool? DuoAgentPlatformEnabled { get; init; }

    public bool? DuoWorkflowMcpEnabled { get; init; }

    public bool? FoundationalAgentsDefaultEnabled { get; init; }

    public bool? AiCatalogRestrictedToGroupHierarchy { get; init; }

    public bool? AiUsageDataCollectionEnabled { get; init; }

    public string? PromptInjectionProtectionLevel { get; init; }

    public bool? IncludeRecommendedAllowed { get; init; }

    public bool? AllowAllUnixSockets { get; init; }

    public bool? AllowProjectExtension { get; init; }

    public string? MinimumAccessLevelExecute { get; init; }

    public string? MinimumAccessLevelExecuteAsync { get; init; }

    public string? MinimumAccessLevelManage { get; init; }

    public string? MinimumAccessLevelEnableOnProjects { get; init; }
}