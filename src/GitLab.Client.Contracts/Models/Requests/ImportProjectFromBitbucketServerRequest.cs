using System.Text;

namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body for <c>POST /import/bitbucket_server</c>, which imports a repository from a self-managed
///     Bitbucket Server (Data Center) instance.
/// </summary>
/// <remarks>
///     <see cref="PersonalAccessToken" /> is a Bitbucket Server credential. Keep it on this record: never
///     log a populated request, and never fold one into a message a user or a log sink will see.
/// </remarks>
public sealed record ImportProjectFromBitbucketServerRequest
{
    /// <summary>The base URL of the Bitbucket Server instance.</summary>
    public required Uri BitbucketServerUrl { get; init; }

    /// <summary>The Bitbucket Server username the token belongs to.</summary>
    public required string BitbucketServerUsername { get; init; }

    /// <summary>A Bitbucket Server personal access token, or the account password. A secret.</summary>
    public required string PersonalAccessToken { get; init; }

    /// <summary>
    ///     The Bitbucket project key holding the repository. Used only to locate the repository on
    ///     Bitbucket; it does not decide where the project lands in GitLab.
    /// </summary>
    public required string BitbucketServerProject { get; init; }

    /// <summary>The Bitbucket repository slug to import.</summary>
    public required string BitbucketServerRepo { get; init; }

    /// <summary>The name of the new project. Defaults to the Bitbucket repository's name.</summary>
    public string? NewName { get; init; }

    /// <summary>The GitLab namespace to import into. Defaults to the caller's own namespace.</summary>
    public string? NewNamespace { get; init; }

    /// <summary>What the importer does when a stage times out. Defaults to pessimistic server-side.</summary>
    public GitLabProjectImportTimeoutStrategy? TimeoutStrategy { get; init; }

    private bool PrintMembers(StringBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Append("BitbucketServerUrl = ").Append(BitbucketServerUrl is null ? "[none]" : "[redacted]")
            .Append(", BitbucketServerUsername = ")
            .Append(string.IsNullOrEmpty(BitbucketServerUsername) ? "[none]" : "[redacted]")
            .Append(", PersonalAccessToken = ")
            .Append(string.IsNullOrEmpty(PersonalAccessToken) ? "[none]" : "[redacted]")
            .Append(", BitbucketServerProject = ").Append(BitbucketServerProject)
            .Append(", BitbucketServerRepo = ").Append(BitbucketServerRepo)
            .Append(", NewName = ").Append(NewName)
            .Append(", NewNamespace = ").Append(NewNamespace)
            .Append(", TimeoutStrategy = ").Append(TimeoutStrategy);

        return true;
    }
}