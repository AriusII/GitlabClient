namespace GitLab.Client.Models;

/// <summary>
///     Body of <c>PUT /integrations/jira_forge/installation</c>: which GitLab instance the Forge
///     installation points at.
/// </summary>
public sealed record UpdateJiraForgeInstallationRequest
{
    /// <summary>
    ///     Base URL of the self-managed GitLab instance the installation should target, capped by GitLab at
    ///     1024 characters. Leave it <see langword="null" /> to point the installation back at GitLab.com -
    ///     the property is omitted from the request body when null, which GitLab reads as "GitLab.com".
    /// </summary>
    public Uri? InstanceUrl { get; init; }
}