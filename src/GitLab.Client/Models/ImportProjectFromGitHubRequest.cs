using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>Body for <c>POST /import/github</c>, which imports a repository from GitHub.</summary>
/// <remarks>
///     <see cref="PersonalAccessToken" /> is a GitHub credential. Keep it on this record: never log a
///     populated request, and never fold one into a message a user or a log sink will see.
/// </remarks>
public sealed record ImportProjectFromGitHubRequest
{
    /// <summary>A GitHub personal access token with read access to the repository. A secret.</summary>
    public required string PersonalAccessToken { get; init; }

    /// <summary>GitHub's numeric id for the repository - not its <c>owner/name</c> path.</summary>
    public required long RepoId { get; init; }

    /// <summary>The namespace or group to import into.</summary>
    public required string TargetNamespace { get; init; }

    /// <summary>The name of the new project. Defaults to the GitHub repository's name.</summary>
    public string? NewName { get; init; }

    /// <summary>
    ///     The base URL of a GitHub Enterprise instance, such as <c>https://github.example.com</c>. Omit
    ///     for github.com.
    /// </summary>
    public Uri? GithubHostname { get; init; }

    /// <summary>
    ///     Which optional stages to run, keyed by stage name (<c>single_endpoint_issue_events_import</c>,
    ///     <c>attachments_import</c>, <c>collaborators_import</c>). The spec types it as an untyped object
    ///     whose members GitLab grows release by release, so it is carried as a raw
    ///     <see cref="JsonElement" />.
    /// </summary>
    public JsonElement? OptionalStages { get; init; }

    /// <summary>What the importer does when a stage times out. Defaults to pessimistic server-side.</summary>
    public GitLabProjectImportTimeoutStrategy? TimeoutStrategy { get; init; }

    /// <summary>Page size the importer uses against the GitHub API, 1-100.</summary>
    public int? PaginationLimit { get; init; }
}