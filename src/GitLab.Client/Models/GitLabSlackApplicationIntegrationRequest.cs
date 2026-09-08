namespace GitLab.Client.Models;

/// <summary>
///     Settings for the GitLab for Slack app integration
///     (<c>PUT /projects/:id/integrations/gitlab-slack-application</c>) - the OAuth-based successor to
///     the Slack webhook integration. Unlike <c>slack</c>, this integration carries no webhook URL: it
///     authenticates through the GitLab for Slack app installed on the workspace, and this request only
///     tunes which channels and events it notifies.
/// </summary>
public sealed record GitLabSlackApplicationIntegrationRequest
{
    /// <summary>Default channel to use if no other channel is configured.</summary>
    public string? Channel { get; init; }

    /// <summary>Send notifications for broken pipelines.</summary>
    public bool? NotifyOnlyBrokenPipelines { get; init; }

    /// <summary>Send notifications only when the pipeline status changes.</summary>
    public bool? NotifyOnlyWhenPipelineStatusChanges { get; init; }

    /// <summary>
    ///     Branches to send notifications for. Valid options are <c>all</c>, <c>default</c>,
    ///     <c>protected</c>, and <c>default_and_protected</c>. Defaults to <c>default</c>.
    /// </summary>
    public string? BranchesToBeNotified { get; init; }

    /// <summary>Labels to send notifications for. Leave blank to receive notifications for all events.</summary>
    public string? LabelsToBeNotified { get; init; }

    /// <summary>
    ///     How <see cref="LabelsToBeNotified" /> is matched. Valid options are <c>match_any</c> and <c>match_all</c>.
    ///     Defaults to <c>match_any</c>.
    /// </summary>
    public string? LabelsToBeNotifiedBehavior { get; init; }

    /// <summary>The name of the channel to receive <see cref="PushEvents" /> notifications.</summary>
    public string? PushChannel { get; init; }

    /// <summary>The name of the channel to receive <see cref="IssuesEvents" /> notifications.</summary>
    public string? IssueChannel { get; init; }

    /// <summary>The name of the channel to receive <see cref="IncidentEvents" /> notifications.</summary>
    public string? IncidentChannel { get; init; }

    /// <summary>The name of the channel to receive <see cref="AlertEvents" /> notifications.</summary>
    public string? AlertChannel { get; init; }

    /// <summary>The name of the channel to receive <see cref="ConfidentialIssuesEvents" /> notifications.</summary>
    public string? ConfidentialIssueChannel { get; init; }

    /// <summary>The name of the channel to receive <see cref="MergeRequestsEvents" /> notifications.</summary>
    public string? MergeRequestChannel { get; init; }

    /// <summary>The name of the channel to receive <see cref="NoteEvents" /> notifications.</summary>
    public string? NoteChannel { get; init; }

    /// <summary>The name of the channel to receive <see cref="ConfidentialNoteEvents" /> notifications.</summary>
    public string? ConfidentialNoteChannel { get; init; }

    /// <summary>The name of the channel to receive <see cref="TagPushEvents" /> notifications.</summary>
    public string? TagPushChannel { get; init; }

    /// <summary>The name of the channel to receive <see cref="DeploymentEvents" /> notifications.</summary>
    public string? DeploymentChannel { get; init; }

    /// <summary>The name of the channel to receive <see cref="PipelineEvents" /> notifications.</summary>
    public string? PipelineChannel { get; init; }

    /// <summary>The name of the channel to receive <see cref="WikiPageEvents" /> notifications.</summary>
    public string? WikiPageChannel { get; init; }

    /// <summary>The name of the channel to receive vulnerability-event notifications.</summary>
    public string? VulnerabilityChannel { get; init; }

    /// <summary>Trigger event for pushes to the repository.</summary>
    public bool? PushEvents { get; init; }

    /// <summary>Trigger event when a work item is created, updated, or closed.</summary>
    public bool? IssuesEvents { get; init; }

    /// <summary>Trigger event when a confidential work item is created, updated, or closed.</summary>
    public bool? ConfidentialIssuesEvents { get; init; }

    /// <summary>Trigger event when a merge request is created, updated, or merged.</summary>
    public bool? MergeRequestsEvents { get; init; }

    /// <summary>Trigger event for new comments.</summary>
    public bool? NoteEvents { get; init; }

    /// <summary>Trigger event for new comments on confidential work items.</summary>
    public bool? ConfidentialNoteEvents { get; init; }

    /// <summary>Trigger event for new tags pushed to the repository.</summary>
    public bool? TagPushEvents { get; init; }

    /// <summary>Trigger event when a pipeline status changes.</summary>
    public bool? PipelineEvents { get; init; }

    /// <summary>Trigger event when a wiki page is created or updated.</summary>
    public bool? WikiPageEvents { get; init; }

    /// <summary>Trigger event when a deployment starts or finishes.</summary>
    public bool? DeploymentEvents { get; init; }

    /// <summary>Trigger event when an incident is created.</summary>
    public bool? IncidentEvents { get; init; }

    public bool? WorkItemEvents { get; init; }

    public bool? ConfidentialWorkItemEvents { get; init; }

    public bool? VulnerabilityEvents { get; init; }

    /// <summary>Trigger event when a new, unique alert is recorded.</summary>
    public bool? AlertEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings. Defaults to <c>false</c>.</summary>
    public bool? UseInheritedSettings { get; init; }
}