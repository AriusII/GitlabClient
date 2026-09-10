namespace GitLab.Client.Models.Requests;

/// <summary>The body of <c>PUT /projects/:id/merge_requests/:merge_request_iid/rebase</c>.</summary>
public sealed record RebaseMergeRequestRequest
{
    /// <summary>Marks the rebase commit so that no pipeline is triggered by it.</summary>
    public bool? SkipCi { get; init; }
}