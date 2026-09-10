namespace GitLab.Client.Models.Requests;

/// <summary>Settings for the Slack integration (<c>PUT /projects/:id/integrations/slack</c>).</summary>
public sealed record SlackIntegrationRequest
{
    /// <summary>Slack incoming webhook URL.</summary>
    public required Uri Webhook { get; init; }

    /// <summary>Username shown by Slack notifications.</summary>
    public string? Username { get; init; }

    /// <summary>Default Slack channel.</summary>
    public string? Channel { get; init; }

    public bool? NotifyOnlyBrokenPipelines { get; init; }
    public bool? NotifyOnlyWhenPipelineStatusChanges { get; init; }
    public string? BranchesToBeNotified { get; init; }
    public string? LabelsToBeNotified { get; init; }
    public string? LabelsToBeNotifiedBehavior { get; init; }
    public string? PushChannel { get; init; }
    public string? IssueChannel { get; init; }
    public string? IncidentChannel { get; init; }
    public string? AlertChannel { get; init; }
    public string? ConfidentialIssueChannel { get; init; }
    public string? MergeRequestChannel { get; init; }
    public string? NoteChannel { get; init; }
    public string? ConfidentialNoteChannel { get; init; }
    public string? TagPushChannel { get; init; }
    public string? DeploymentChannel { get; init; }
    public string? PipelineChannel { get; init; }
    public string? WikiPageChannel { get; init; }
    public string? VulnerabilityChannel { get; init; }
    public bool? PushEvents { get; init; }
    public bool? IssuesEvents { get; init; }
    public bool? ConfidentialIssuesEvents { get; init; }
    public bool? MergeRequestsEvents { get; init; }
    public bool? NoteEvents { get; init; }
    public bool? ConfidentialNoteEvents { get; init; }
    public bool? TagPushEvents { get; init; }
    public bool? PipelineEvents { get; init; }
    public bool? WikiPageEvents { get; init; }
    public bool? DeploymentEvents { get; init; }
    public bool? IncidentEvents { get; init; }
    public bool? WorkItemEvents { get; init; }
    public bool? ConfidentialWorkItemEvents { get; init; }
    public bool? VulnerabilityEvents { get; init; }
    public bool? AlertEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings.</summary>
    public bool? UseInheritedSettings { get; init; }
}