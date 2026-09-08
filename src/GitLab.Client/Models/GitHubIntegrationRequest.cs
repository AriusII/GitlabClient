namespace GitLab.Client.Models;

/// <summary>
///     Settings for the GitHub integration (<c>PUT /projects/:id/integrations/github</c>) - mirrors
///     pipeline status back to a GitHub repository.
/// </summary>
public sealed record GitHubIntegrationRequest
{
    /// <summary>GitHub API token with the <c>repo:status</c> OAuth scope.</summary>
    public required string Token { get; init; }

    /// <summary>GitHub repository URL.</summary>
    public required Uri RepositoryUrl { get; init; }

    /// <summary>Append the hostname of your GitLab instance to the status check name.</summary>
    public bool? StaticContext { get; init; }

    /// <summary>Trigger event when a pipeline status changes.</summary>
    public bool? PipelineEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings. Defaults to <c>false</c>.</summary>
    public bool? UseInheritedSettings { get; init; }
}