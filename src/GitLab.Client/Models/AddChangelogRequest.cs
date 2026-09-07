namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects/:id/repository/changelog</c>, which generates changelog data and
///     commits it to a file in the repository. The <c>GET</c> sibling produces the same text without
///     writing anything.
/// </summary>
public sealed record AddChangelogRequest
{
    /// <summary>The release version, in semantic-versioning form.</summary>
    public required string Version { get; init; }

    /// <summary>
    ///     The first commit of the range to summarize. GitLab infers it from the previous tag when unset.
    /// </summary>
    public string? From { get; init; }

    /// <summary>The last commit of the range to summarize. Defaults to the branch being committed to.</summary>
    public string? To { get; init; }

    /// <summary>The release timestamp. Defaults to now.</summary>
    public DateTimeOffset? Date { get; init; }

    /// <summary>The Git trailer that marks a commit for inclusion. Defaults to <c>Changelog</c>.</summary>
    public string? Trailer { get; init; }

    /// <summary>Path to the changelog configuration. Defaults to <c>.gitlab/changelog_config.yml</c>.</summary>
    public string? ConfigFile { get; init; }

    /// <summary>The ref the configuration file is read from. Defaults to the default branch.</summary>
    public string? ConfigFileRef { get; init; }

    /// <summary>The branch to commit the changelog to. Defaults to the default branch.</summary>
    public string? Branch { get; init; }

    /// <summary>The file the changelog is written to. Defaults to <c>CHANGELOG.md</c>.</summary>
    public string? File { get; init; }

    /// <summary>The commit message to use. GitLab generates one when unset.</summary>
    public string? Message { get; init; }
}