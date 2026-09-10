namespace GitLab.Client.Models.Requests;

/// <summary>Settings for the Phorge integration (<c>PUT /projects/:id/integrations/phorge</c>).</summary>
public sealed record PhorgeIntegrationRequest
{
    /// <summary>URL of the Phorge project.</summary>
    public required Uri ProjectUrl { get; init; }

    /// <summary>URL template for a Phorge issue.</summary>
    public required Uri IssuesUrl { get; init; }

    /// <summary>Triggers events for pushes to the repository.</summary>
    public bool? PushEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings.</summary>
    public bool? UseInheritedSettings { get; init; }
}