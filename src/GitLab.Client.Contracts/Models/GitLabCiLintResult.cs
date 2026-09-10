using System.Text.Json;

using GitLab.Client.Models.Requests;

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

    /// <summary>Why the configuration is invalid, one message per problem. Empty (not null) when <see cref="Valid" /> is true.</summary>
    public IReadOnlyList<string>? Errors { get; init; }

    /// <summary>Non-fatal problems with an otherwise valid configuration.</summary>
    public IReadOnlyList<string>? Warnings { get; init; }

    /// <summary>
    ///     The fully expanded configuration after every <c>include:</c> and <c>extends:</c> has been resolved.
    ///     Can be very large, and can contain values worth keeping out of logs.
    /// </summary>
    public string? MergedYaml { get; init; }

    public IReadOnlyList<GitLabCiLintInclude>? Includes { get; init; }

    /// <summary>
    ///     The jobs the configuration would produce, present only when the request opted in via
    ///     <see cref="CiLintOptions.IncludeJobs" /> or <see cref="ValidateCiConfigurationRequest.IncludeJobs" />.
    ///     GitLab's spec leaves each job's shape unspecified, so entries are surfaced as raw
    ///     <see cref="JsonElement" /> rather than an invented DTO - the same convention
    ///     <see cref="CiCatalogPublishRequest.Metadata" /> uses for its own untyped object.
    /// </summary>
    public IReadOnlyList<JsonElement>? Jobs { get; init; }
}