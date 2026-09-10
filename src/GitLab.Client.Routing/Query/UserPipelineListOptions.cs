using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Query;

/// <summary>
///     Filters for listing the pipelines the authenticated user triggered across every project
///     (<c>GET /pipelines</c>).
///     <para>
///         This endpoint is keyset-paginated rather than offset-paginated: it accepts
///         <see cref="Cursor" /> and <see cref="PerPage" /> but no <c>page</c>, and it only orders by
///         <c>created_at</c> descending. Streaming the result follows GitLab's <c>Link: rel="next"</c>
///         header, so the cursor rarely needs setting by hand.
///     </para>
/// </summary>
[GitLabQuery]
public readonly record struct UserPipelineListOptions
{
    /// <summary>
    ///     What started the pipeline - "push", "web", "trigger", "schedule", "api", "external", "pipeline",
    ///     "chat", "webide", "merge_request_event", "external_pull_request_event" or "unknown".
    /// </summary>
    public string? Source { get; init; }

    public DateTimeOffset? CreatedBefore { get; init; }

    public DateTimeOffset? CreatedAfter { get; init; }

    /// <summary>Only <c>created_at</c> is accepted here, which is also the default.</summary>
    public string? OrderBy { get; init; }

    /// <summary>Only <c>desc</c> is accepted here, which is also the default.</summary>
    public string? Sort { get; init; }

    /// <summary>An opaque keyset cursor for resuming a listing; normally left unset.</summary>
    public string? Cursor { get; init; }

    public int? PerPage { get; init; }
}