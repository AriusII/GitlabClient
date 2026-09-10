namespace GitLab.Client.Models.Requests;

/// <summary>
///     Settings for the Pivotal Tracker integration
///     (<c>PUT /projects/:id/integrations/pivotaltracker</c>).
/// </summary>
public sealed record PivotalTrackerIntegrationRequest
{
    /// <summary>Pivotal Tracker API token.</summary>
    public required string Token { get; init; }

    /// <summary>Comma-separated branches to inspect automatically.</summary>
    public string? RestrictToBranch { get; init; }

    /// <summary>Triggers events for pushes to the repository.</summary>
    public bool? PushEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings.</summary>
    public bool? UseInheritedSettings { get; init; }
}