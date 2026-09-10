namespace GitLab.Client.Models.Requests;

/// <summary>Settings for the Pumble integration (<c>PUT /projects/:id/integrations/pumble</c>).</summary>
public sealed record PumbleIntegrationRequest
{
    /// <summary>Pumble webhook URL.</summary>
    public required Uri Webhook { get; init; }

    /// <summary>Sends notifications only for broken pipelines.</summary>
    public bool? NotifyOnlyBrokenPipelines { get; init; }

    /// <summary>Sends notifications only when the pipeline status changes.</summary>
    public bool? NotifyOnlyWhenPipelineStatusChanges { get; init; }

    /// <summary>Branches for which notifications are sent.</summary>
    public string? BranchesToBeNotified { get; init; }

    public bool? PushEvents { get; init; }
    public bool? IssuesEvents { get; init; }
    public bool? ConfidentialIssuesEvents { get; init; }
    public bool? WorkItemEvents { get; init; }
    public bool? ConfidentialWorkItemEvents { get; init; }
    public bool? MergeRequestsEvents { get; init; }
    public bool? NoteEvents { get; init; }
    public bool? ConfidentialNoteEvents { get; init; }
    public bool? TagPushEvents { get; init; }
    public bool? PipelineEvents { get; init; }
    public bool? WikiPageEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings.</summary>
    public bool? UseInheritedSettings { get; init; }
}