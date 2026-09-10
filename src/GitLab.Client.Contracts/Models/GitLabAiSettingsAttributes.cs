namespace GitLab.Client.Models;

/// <summary>
///     The <c>ai_settings_attributes</c> object accepted by <c>PUT /groups/:id</c>.
///     Every member is optional so an update changes only the AI setting it names.
/// </summary>
public sealed record GitLabAiSettingsAttributes
{
    public bool? DuoAgentPlatformEnabled { get; init; }

    public bool? DuoWorkflowMcpEnabled { get; init; }

    public bool? AiUsageDataCollectionEnabled { get; init; }

    public bool? AiCatalogRestrictedToGroupHierarchy { get; init; }

    public bool? FoundationalAgentsDefaultEnabled { get; init; }

    public GitLabPromptInjectionProtectionLevel? PromptInjectionProtectionLevel { get; init; }

    public bool? IncludeRecommendedAllowed { get; init; }

    public bool? AllowAllUnixSockets { get; init; }

    public bool? AllowProjectExtension { get; init; }

    /// <summary>Minimum GitLab access level allowed to execute Duo Agent Platform features.</summary>
    public int? MinimumAccessLevelExecute { get; init; }

    /// <summary>Minimum GitLab access level allowed to execute Duo Agent Platform features in CI/CD.</summary>
    public int? MinimumAccessLevelExecuteAsync { get; init; }

    /// <summary>Minimum GitLab access level allowed to manage Duo Agent Platform settings.</summary>
    public int? MinimumAccessLevelManage { get; init; }

    /// <summary>Minimum GitLab access level allowed to enable Duo Agent Platform on projects.</summary>
    public int? MinimumAccessLevelEnableOnProjects { get; init; }
}