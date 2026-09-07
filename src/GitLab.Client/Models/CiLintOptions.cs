using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Query parameters for <c>GET /projects/:id/ci/lint</c>, which validates the configuration already
///     committed to a project. The spec's <c>sha</c> and <c>ref</c> parameters are deliberately absent:
///     both are marked deprecated in favour of <see cref="ContentRef" /> and <see cref="DryRunRef" />.
/// </summary>
[GitLabQuery]
public sealed record CiLintOptions
{
    /// <summary>The commit, branch or tag to read the configuration from. Defaults to the default branch's HEAD.</summary>
    public string? ContentRef { get; init; }

    /// <summary>Simulate pipeline creation instead of only checking the syntax. Defaults to false.</summary>
    public bool? DryRun { get; init; }

    /// <summary>The branch or tag used as context for the dry run. Only honoured when <see cref="DryRun" /> is true.</summary>
    public string? DryRunRef { get; init; }

    /// <summary>Include the jobs the configuration would produce in the response. Defaults to false.</summary>
    public bool? IncludeJobs { get; init; }
}