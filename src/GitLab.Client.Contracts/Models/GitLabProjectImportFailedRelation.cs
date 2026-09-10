namespace GitLab.Client.Models;

/// <summary>
///     One relation an import could not restore, as reported by
///     <see cref="GitLabProjectImportStatus.FailedRelations" />. An import can finish with failed
///     relations: the project exists, but the listed pieces of it did not make it across.
/// </summary>
public sealed record GitLabProjectImportFailedRelation
{
    public required long Id { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>The Ruby exception class GitLab raised, such as <c>StandardError</c>.</summary>
    public string? ExceptionClass { get; init; }

    /// <summary>The worker or step that raised it, such as <c>ImportRepositoryWorker</c>.</summary>
    public string? Source { get; init; }

    /// <summary>The exception message. Truncated by GitLab, and may be null when it withheld the detail.</summary>
    public string? ExceptionMessage { get; init; }

    /// <summary>The relation that failed - <c>issues</c>, <c>merge_requests</c>, and so on.</summary>
    public string? RelationName { get; init; }

    /// <summary>The line in the export's NDJSON file for this relation that could not be restored.</summary>
    public int? LineNumber { get; init; }
}