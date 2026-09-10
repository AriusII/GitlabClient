namespace GitLab.Client.Models;

/// <summary>
///     One record a migration entity could not import
///     (<c>GET /bulk_imports/:import_id/entities/:entity_id/failures</c>).
/// </summary>
public sealed record GitLabBulkImportEntityFailure
{
    /// <summary>The relation the failed record belongs to - <c>label</c>, <c>issue</c>, <c>milestone</c>, ...</summary>
    public string? Relation { get; init; }

    /// <summary>The exception message GitLab recorded.</summary>
    public string? ExceptionMessage { get; init; }

    /// <summary>The Ruby exception class GitLab recorded.</summary>
    public string? ExceptionClass { get; init; }

    /// <summary>The correlation ID of the failing job, for matching this against the instance's logs.</summary>
    public string? CorrelationIdValue { get; init; }

    /// <summary>Where the record lives on the source instance.</summary>
    public Uri? SourceUrl { get; init; }

    /// <summary>The title of the record on the source instance.</summary>
    public string? SourceTitle { get; init; }
}