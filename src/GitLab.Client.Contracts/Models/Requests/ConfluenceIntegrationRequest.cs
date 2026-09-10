namespace GitLab.Client.Models.Requests;

/// <summary>
///     Settings for the Confluence Workspace integration
///     (<c>PUT /projects/:id/integrations/confluence</c>) - replaces the project wiki link with a
///     Confluence space.
/// </summary>
public sealed record ConfluenceIntegrationRequest
{
    /// <summary>URL of the Confluence Workspace hosted on <c>atlassian.net</c>.</summary>
    public required Uri ConfluenceUrl { get; init; }

    /// <summary>Indicates whether to inherit the default settings. Defaults to <c>false</c>.</summary>
    public bool? UseInheritedSettings { get; init; }
}