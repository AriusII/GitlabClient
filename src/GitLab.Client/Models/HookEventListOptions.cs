using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for listing a webhook's delivery log
///     (<c>GET /projects/:id/hooks/:hook_id/events</c>, <c>GET /groups/:id/hooks/:hook_id/events</c>).
/// </summary>
[GitLabQuery]
public sealed record HookEventListOptions
{
    /// <summary>
    ///     Response statuses to include. Each element is either an exact HTTP status code as text
    ///     (<c>"200"</c>, <c>"500"</c>) or one of the buckets <c>"successful"</c>, <c>"client_failure"</c> and
    ///     <c>"server_failure"</c>. The spec declares this as a non-exploded form array, which is the
    ///     comma-joined default (<c>status=500,502</c>).
    /// </summary>
    public IReadOnlyList<string>? Status { get; init; }

    /// <summary>Page size. GitLab caps this endpoint at 20, unlike the usual 100.</summary>
    public int? PerPage { get; init; }
}