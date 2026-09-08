using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Match filters for the bulk tag-deletion endpoint
///     (<c>DELETE /projects/:id/registry/repositories/:repository_id/tags</c>). GitLab requires at least
///     one filter to be set, or it rejects the request rather than deleting every tag by accident.
/// </summary>
[GitLabQuery]
public readonly record struct DeleteRegistryRepositoryTagsOptions
{
    /// <summary>The tag name regexp to delete; pass <c>.*</c> to match every tag.</summary>
    public string? NameRegexDelete { get; init; }

    /// <summary>
    ///     Older alias for <see cref="NameRegexDelete" />. The spec still declares both names on this
    ///     route, so both are exposed here rather than picking one and silently dropping the other.
    /// </summary>
    public string? NameRegex { get; init; }

    /// <summary>Tag name regexp to retain even if it matches a delete filter above.</summary>
    public string? NameRegexKeep { get; init; }

    /// <summary>Keep this many of the most recent tags matching <see cref="NameRegexKeep" />.</summary>
    public int? KeepN { get; init; }

    /// <summary>Delete only tags older than this duration, e.g. <c>1h</c>, <c>1d</c>, <c>1month</c>.</summary>
    public string? OlderThan { get; init; }
}