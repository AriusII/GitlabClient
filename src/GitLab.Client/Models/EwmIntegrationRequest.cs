namespace GitLab.Client.Models;

/// <summary>
///     Settings for the IBM Engineering Workflow Management integration
///     (<c>PUT /projects/:id/integrations/ewm</c>) - uses EWM as the project's external issue tracker.
/// </summary>
public sealed record EwmIntegrationRequest
{
    /// <summary>URL of the project.</summary>
    public required Uri ProjectUrl { get; init; }

    /// <summary>URL of the issue.</summary>
    public required Uri IssuesUrl { get; init; }

    /// <summary>URL of the new issue.</summary>
    public required Uri NewIssueUrl { get; init; }

    /// <summary>Trigger event for pushes to the repository.</summary>
    public bool? PushEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings. Defaults to <c>false</c>.</summary>
    public bool? UseInheritedSettings { get; init; }
}