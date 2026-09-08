namespace GitLab.Client.Models;

/// <summary>
///     Settings for the Webex Teams integration (<c>PUT /projects/:id/integrations/webex-teams</c>) -
///     post events to a Webex space.
/// </summary>
public sealed record WebexTeamsIntegrationRequest
{
    /// <summary>The Webex Teams webhook. For example, <c>https://api.ciscospark.com/v1/webhooks/incoming/...</c>.</summary>
    public required Uri Webhook { get; init; }

    /// <summary>Send notifications for broken pipelines.</summary>
    public bool? NotifyOnlyBrokenPipelines { get; init; }

    /// <summary>Send notifications only when the pipeline status changes.</summary>
    public bool? NotifyOnlyWhenPipelineStatusChanges { get; init; }

    /// <summary>
    ///     Branches to send notifications for. Valid options are <c>all</c>, <c>default</c>,
    ///     <c>protected</c>, and <c>default_and_protected</c>. The default value is <c>default</c>.
    /// </summary>
    public string? BranchesToBeNotified { get; init; }

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

    /// <summary>Indicates whether to inherit the default settings. Defaults to <c>false</c>.</summary>
    public bool? UseInheritedSettings { get; init; }
}