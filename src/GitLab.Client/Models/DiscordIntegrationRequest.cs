namespace GitLab.Client.Models;

/// <summary>
///     Settings for the Discord Notifications integration
///     (<c>PUT /projects/:id/integrations/discord</c>) - posts events to a Discord channel webhook.
///     <para>
///         The per-event <c>*Channel</c> members override <see cref="Webhook" />'s default channel for
///         that specific event type; leaving one unset falls back to the webhook's own channel.
///     </para>
/// </summary>
public sealed record DiscordIntegrationRequest
{
    /// <summary>Discord webhook (for example, <c>https://discord.com/api/webhooks/...</c>).</summary>
    public required string Webhook { get; init; }

    /// <summary>Send notifications for broken pipelines.</summary>
    public bool? NotifyOnlyBrokenPipelines { get; init; }

    /// <summary>Send notifications only when the pipeline status changes.</summary>
    public bool? NotifyOnlyWhenPipelineStatusChanges { get; init; }

    /// <summary>
    ///     Branches to send notifications for. Valid options are <c>all</c>, <c>default</c>,
    ///     <c>protected</c>, and <c>default_and_protected</c>. Defaults to <c>default</c>.
    /// </summary>
    public string? BranchesToBeNotified { get; init; }

    /// <summary>The name of the channel to receive <see cref="PushEvents" /> notifications.</summary>
    public string? PushChannel { get; init; }

    /// <summary>The name of the channel to receive <see cref="IssuesEvents" /> notifications.</summary>
    public string? IssueChannel { get; init; }

    /// <summary>The name of the channel to receive <c>IncidentEvents</c> notifications.</summary>
    public string? IncidentChannel { get; init; }

    /// <summary>The name of the channel to receive <c>AlertEvents</c> notifications.</summary>
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

    public bool? WorkItemEvents { get; init; }

    public bool? ConfidentialWorkItemEvents { get; init; }

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

    /// <summary>Indicates whether to inherit the default settings. Defaults to <c>false</c>.</summary>
    public bool? UseInheritedSettings { get; init; }
}