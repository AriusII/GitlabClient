namespace GitLab.Client.Models;

/// <summary>
///     Typed settings for the <c>jira-cloud-app</c> integration (the GitLab for Jira Cloud app) - see
///     <see cref="GitLabIntegrationSlug.JiraCloudApp" />. Unlike <see cref="JiraIntegrationRequest" />,
///     the spec declares no required members here at all.
/// </summary>
public sealed record JiraCloudAppIntegrationRequest
{
    /// <summary>
    ///     Your Jira Service Management (JSM) Service ID(s). Use a comma (<c>,</c>) to separate multiple
    ///     IDs.
    /// </summary>
    public string? JiraCloudAppServiceIds { get; init; }

    /// <summary>Enable to approve or reject blocked GitLab deployments from Jira Service Management.</summary>
    public bool? JiraCloudAppEnableDeploymentGating { get; init; }

    /// <summary>
    ///     The environment(s) (for example, <c>production</c>, <c>staging</c>, <c>testing</c>,
    ///     <c>development</c>) where deployment gating is enabled.
    /// </summary>
    public string? JiraCloudAppDeploymentGatingEnvironments { get; init; }

    /// <summary>Indicates whether to inherit the default settings. Defaults to <see langword="false" />.</summary>
    public bool? UseInheritedSettings { get; init; }
}