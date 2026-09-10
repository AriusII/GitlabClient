namespace GitLab.Client.Models.Requests;

/// <summary>
///     Typed settings for the <c>matrix</c> integration - see
///     <see cref="GitLabIntegrationSlug.Matrix" />.
/// </summary>
public sealed record MatrixIntegrationRequest
{
    /// <summary>
    ///     Custom hostname of the Matrix server. The default value is
    ///     <c>https://matrix-client.matrix.org</c>.
    /// </summary>
    public Uri? Hostname { get; init; }

    /// <summary>The Matrix access token (for example, <c>syt-zyx57W2v1u123ew11</c>).</summary>
    public required string Token { get; init; }

    /// <summary>
    ///     Unique identifier for the target room (in the format
    ///     <c>!qPKKM111FFKKsfoCVy:matrix.org</c>).
    /// </summary>
    public required string Room { get; init; }

    /// <summary>Send notifications for broken pipelines.</summary>
    public bool? NotifyOnlyBrokenPipelines { get; init; }

    /// <summary>Send notifications only when the pipeline status changes.</summary>
    public bool? NotifyOnlyWhenPipelineStatusChanges { get; init; }

    /// <summary>
    ///     Branches to send notifications for. Valid values are <c>all</c>, <c>default</c>,
    ///     <c>protected</c>, and <c>default_and_protected</c>.
    /// </summary>
    public string? BranchesToBeNotified { get; init; }

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

    /// <summary>Trigger event when an incident is created.</summary>
    public bool? IncidentEvents { get; init; }

    public bool? WorkItemEvents { get; init; }

    public bool? ConfidentialWorkItemEvents { get; init; }

    public bool? VulnerabilityEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings. Defaults to <see langword="false" />.</summary>
    public bool? UseInheritedSettings { get; init; }
}