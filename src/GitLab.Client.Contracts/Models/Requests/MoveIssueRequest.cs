namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /projects/:id/issues/:issue_iid/move</c>. The issue is closed in the source
///     project and recreated in the target one, which is why the call answers with the <i>new</i> issue.
/// </summary>
public sealed record MoveIssueRequest
{
    /// <summary>The numeric id of the project to move the issue into.</summary>
    public required long ToProjectId { get; init; }
}