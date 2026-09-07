namespace GitLab.Client.Models;

/// <summary>Body for <c>POST /import/bitbucket</c>, which imports a repository from Bitbucket Cloud.</summary>
/// <remarks>
///     <see cref="BitbucketApiToken" /> is a Bitbucket credential. Keep it on this record: never log a
///     populated request, and never fold one into a message a user or a log sink will see.
/// </remarks>
public sealed record ImportProjectFromBitbucketRequest
{
    /// <summary>The Atlassian account email the API token belongs to.</summary>
    public required string BitbucketEmail { get; init; }

    /// <summary>A Bitbucket Cloud API token with read access to the repository. A secret.</summary>
    public required string BitbucketApiToken { get; init; }

    /// <summary>The repository to import, as <c>workspace/repository</c>.</summary>
    public required string RepoPath { get; init; }

    /// <summary>The GitLab namespace or group to import into.</summary>
    public required string TargetNamespace { get; init; }

    /// <summary>The name of the new project. Defaults to the Bitbucket repository's name.</summary>
    public string? NewName { get; init; }
}