using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     A to-do item in the authenticated user's GitLab inbox, as returned by the To-dos API
///     (<c>/todos</c>).
/// </summary>
public sealed record GitLabTodo
{
    public required long Id { get; init; }

    /// <summary>The project the to-do item belongs to; null for a group-scoped item.</summary>
    public GitLabProjectIdentity? Project { get; init; }

    /// <summary>The group the to-do item belongs to; null for a project-scoped item.</summary>
    public GitLabNamespace? Group { get; init; }

    /// <summary>Who caused the to-do item to appear - the assigner, mentioner, or reviewer requester.</summary>
    public GitLabUser? Author { get; init; }

    /// <summary>
    ///     Why the item exists: <c>assigned</c>, <c>review_requested</c>, <c>mentioned</c>,
    ///     <c>build_failed</c>, <c>marked</c>, <c>approval_required</c>, <c>unmergeable</c>,
    ///     <c>directly_addressed</c>, <c>member_access_requested</c>, <c>review_submitted</c>,
    ///     <c>ssh_key_expired</c> or <c>ssh_key_expiring_soon</c>.
    /// </summary>
    public string? ActionName { get; init; }

    /// <summary>
    ///     What the item points at: <c>Commit</c>, <c>Issue</c>, <c>WorkItem</c>, <c>MergeRequest</c>,
    ///     <c>DesignManagement::Design</c>, <c>AlertManagement::Alert</c>, <c>Namespace</c>, <c>Project</c>,
    ///     <c>Key</c>, <c>WikiPage::Meta</c>, <c>Epic</c> or <c>Vulnerability</c>. The <c>target</c> object
    ///     itself is untyped in GitLab's spec and is deliberately not modelled - discriminate on this and
    ///     follow <see cref="TargetUrl" />.
    /// </summary>
    public string? TargetType { get; init; }

    /// <summary>
    ///     The complete target object. Its shape varies with <see cref="TargetType" /> and the GitLab 19.4
    ///     schema deliberately leaves it open, so its original JSON is retained rather than forced into an
    ///     incomplete common DTO.
    /// </summary>
    public JsonElement? Target { get; init; }

    public Uri? TargetUrl { get; init; }

    public string? Body { get; init; }

    /// <summary>Either <c>pending</c> or <c>done</c>.</summary>
    public string? State { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}