using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     One configuration file pulled in by an <c>include:</c> directive while linting, as reported in
///     <see cref="GitLabCiLintResult.Includes" />.
/// </summary>
public sealed record GitLabCiLintInclude
{
    /// <summary>How the file was included - <c>local</c>, <c>file</c>, <c>remote</c>, <c>template</c>, <c>component</c>.</summary>
    public string? Type { get; init; }

    /// <summary>The included path, for example <c>.gitlab/ci/build-images.gitlab-ci.yml</c>.</summary>
    public string? Location { get; init; }

    public Uri? Blob { get; init; }

    public Uri? Raw { get; init; }

    /// <summary>
    ///     Include-type-specific details returned by GitLab. Its schema is intentionally open-ended (for
    ///     example, a component include can supply a job name, project and ref), so it is retained losslessly.
    /// </summary>
    public JsonElement? Extra { get; init; }

    /// <summary>The project the file was resolved against, as <c>namespace/project</c>.</summary>
    public string? ContextProject { get; init; }

    /// <summary>The commit the file was read at.</summary>
    public string? ContextSha { get; init; }
}