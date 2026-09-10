namespace GitLab.Client.Models.Requests;

/// <summary>
///     Settings for the Custom Issue Tracker integration
///     (<c>PUT /projects/:id/integrations/custom-issue-tracker</c>) - a custom external issue tracker
///     addressed by URL templates.
/// </summary>
public sealed record CustomIssueTrackerIntegrationRequest
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