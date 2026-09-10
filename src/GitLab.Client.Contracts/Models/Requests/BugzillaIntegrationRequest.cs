namespace GitLab.Client.Models.Requests;

/// <summary>Settings for the Bugzilla integration (<c>PUT /projects/:id/integrations/bugzilla</c>).</summary>
public sealed record BugzillaIntegrationRequest
{
    /// <summary>URL of the Bugzilla project.</summary>
    public required Uri ProjectUrl { get; init; }

    /// <summary>URL template for an issue.</summary>
    public required Uri IssuesUrl { get; init; }

    /// <summary>URL used to create a new issue.</summary>
    public required Uri NewIssueUrl { get; init; }

    /// <summary>Triggers events for pushes to the repository.</summary>
    public bool? PushEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings.</summary>
    public bool? UseInheritedSettings { get; init; }
}