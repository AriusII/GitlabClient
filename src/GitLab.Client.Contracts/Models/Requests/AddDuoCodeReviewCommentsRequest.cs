namespace GitLab.Client.Models.Requests;

/// <summary>Body of <c>POST /ai/duo_workflows/code_review/add_comments</c>.</summary>
public sealed record AddDuoCodeReviewCommentsRequest
{
    /// <summary>The ID or path of the project. Sent in the body, so pass it unencoded.</summary>
    public required string ProjectId { get; init; }

    /// <summary>The IID of the merge request the comments belong to.</summary>
    public required long MergeRequestIid { get; init; }

    /// <summary>The review output produced by the model.</summary>
    public required string ReviewOutput { get; init; }

    /// <summary>The Duo flow session the review came from.</summary>
    public long? WorkflowId { get; init; }
}