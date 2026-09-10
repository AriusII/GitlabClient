namespace GitLab.Client.Models.Requests;

/// <summary>Settings for the ClickUp integration (<c>PUT /projects/:id/integrations/clickup</c>).</summary>
public sealed record ClickUpIntegrationRequest
{
    /// <summary>URL of the ClickUp project.</summary>
    public required Uri ProjectUrl { get; init; }

    /// <summary>URL template for a ClickUp issue.</summary>
    public required Uri IssuesUrl { get; init; }

    /// <summary>Triggers events for pushes to the repository.</summary>
    public bool? PushEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings.</summary>
    public bool? UseInheritedSettings { get; init; }
}