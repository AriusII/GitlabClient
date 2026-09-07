using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters for listing the authenticated user's to-do items (<c>GET /todos</c>).</summary>
[GitLabQuery]
public sealed record TodoListOptions
{
    /// <summary>
    ///     One of <c>assigned</c>, <c>review_requested</c>, <c>mentioned</c>, <c>build_failed</c>,
    ///     <c>marked</c>, <c>approval_required</c>, <c>unmergeable</c>, <c>directly_addressed</c>,
    ///     <c>member_access_requested</c>, <c>review_submitted</c>, <c>ssh_key_expired</c> or
    ///     <c>ssh_key_expiring_soon</c>.
    /// </summary>
    public string? Action { get; init; }

    public long? AuthorId { get; init; }

    public long? ProjectId { get; init; }

    public long? GroupId { get; init; }

    /// <summary>Either <c>pending</c> (the default when omitted) or <c>done</c>.</summary>
    public string? State { get; init; }

    /// <summary>
    ///     One of <c>Commit</c>, <c>Issue</c>, <c>WorkItem</c>, <c>MergeRequest</c>,
    ///     <c>DesignManagement::Design</c>, <c>AlertManagement::Alert</c>, <c>Namespace</c>, <c>Project</c>,
    ///     <c>Key</c>, <c>WikiPage::Meta</c>, <c>Epic</c> or <c>Vulnerability</c>.
    /// </summary>
    public string? Type { get; init; }

    public int? PerPage { get; init; }
}