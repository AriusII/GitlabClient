namespace GitLab.Client.Models.Requests;

/// <summary>
///     Settings for the YouTrack integration (<c>PUT /projects/:id/integrations/youtrack</c>) - use
///     YouTrack as the project's external issue tracker.
/// </summary>
public sealed record YouTrackIntegrationRequest
{
    /// <summary>URL of the project.</summary>
    public required Uri ProjectUrl { get; init; }

    /// <summary>URL of the issue.</summary>
    public required Uri IssuesUrl { get; init; }

    /// <summary>Trigger event for pushes to the repository.</summary>
    public bool? PushEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings. Defaults to <c>false</c>.</summary>
    public bool? UseInheritedSettings { get; init; }
}