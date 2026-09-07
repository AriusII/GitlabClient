namespace GitLab.Client.Models;

/// <summary>Body of <c>POST /ai/duo_workflows/tools/risk_classification/results</c>.</summary>
public sealed record SubmitDuoWorkflowRiskClassificationRequest
{
    /// <summary>The ID or path of the project. Sent in the body, so pass it unencoded.</summary>
    public required string ProjectId { get; init; }

    /// <summary>The IID of the merge request the claims were assessed against.</summary>
    public required long MergeRequestIid { get; init; }

    /// <summary>The full 40-character SHA of the diff revision the claims were assessed against.</summary>
    public required string DiffSha { get; init; }

    /// <summary>The categorical claims about the merge request.</summary>
    public required IReadOnlyList<DuoWorkflowRiskClassificationClaim> Claims { get; init; }

    /// <summary>Plain-language explanation of the change and where its risk lies. At most 2048 characters.</summary>
    public string? Summary { get; init; }
}