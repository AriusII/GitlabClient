namespace GitLab.Client.Models;

/// <summary>
///     Optional request body for <c>POST /projects/:id/merge_requests/:iid/approve</c>. Every member is
///     optional, so the body may be omitted entirely.
/// </summary>
public sealed record ApproveMergeRequestRequest
{
    /// <summary>
    ///     When set, must equal the source branch's HEAD SHA; a mismatch is answered with <c>409 Conflict</c>
    ///     (a <c>GitLabConflictException</c>).
    /// </summary>
    public string? Sha { get; init; }

    /// <summary>When <see langword="true" />, submits the caller's pending review comments alongside the approval.</summary>
    public bool? PublishReview { get; init; }

    /// <summary>The caller's password, for projects configured to require explicit authentication on approval.</summary>
    public string? ApprovalPassword { get; init; }

    /// <summary>
    ///     Deliberately opaque. A record's compiler-generated <c>ToString()</c> prints every member, which would
    ///     put <see cref="ApprovalPassword" /> into any log line or exception message that formats this request.
    /// </summary>
    public override string ToString()
    {
        return nameof(ApproveMergeRequestRequest);
    }
}