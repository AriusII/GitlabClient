namespace GitLab.Client.Models;

/// <summary>
///     The verdict on a CI/CD configuration, as returned by both <c>/projects/:id/ci/lint</c> endpoints.
///     Nothing here is guaranteed: GitLab answers an invalid configuration with <c>200 OK</c> carrying
///     only <see cref="Valid" /> and <see cref="Errors" />.
/// </summary>
public sealed record GitLabCiLintResult
{
    /// <summary>Whether the configuration is valid. Check this - a successful call does not mean valid YAML.</summary>
    public bool? Valid { get; init; }

    public IReadOnlyList<string>? Errors { get; init; }

    public IReadOnlyList<string>? Warnings { get; init; }

    /// <summary>
    ///     The fully expanded configuration after every <c>include:</c> and <c>extends:</c> has been resolved.
    ///     Can be very large, and can contain values worth keeping out of logs.
    /// </summary>
    public string? MergedYaml { get; init; }

    public IReadOnlyList<GitLabCiLintInclude>? Includes { get; init; }
}