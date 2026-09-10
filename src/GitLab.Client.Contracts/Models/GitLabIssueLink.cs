namespace GitLab.Client.Models;

/// <summary>
///     A relation between two issues, as returned by the issue-links endpoints
///     (<c>/projects/:id/issues/:issue_iid/links</c>).
///     <para>
///         Note the asymmetry in GitLab's own API: creating, retrieving or deleting one link answers with
///         this type, while <i>listing</i> them answers with the linked issues themselves, each carrying
///         <see cref="GitLabIssue.IssueLinkId" /> and <see cref="GitLabIssue.LinkType" />.
///     </para>
/// </summary>
public sealed record GitLabIssueLink
{
    /// <summary>The link's own id - what the delete and retrieve routes take as <c>issue_link_id</c>.</summary>
    public required long Id { get; init; }

    public GitLabIssue? SourceIssue { get; init; }

    public GitLabIssue? TargetIssue { get; init; }

    /// <summary>
    ///     <c>relates_to</c>, <c>blocks</c> or <c>is_blocked_by</c>. Typed as a string on purpose: the spec
    ///     leaves the response field unenumerated, and <see cref="GitLabIssueLinkType" /> is the request-side
    ///     vocabulary.
    /// </summary>
    public string? LinkType { get; init; }
}