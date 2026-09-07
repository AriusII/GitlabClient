namespace GitLab.Client.Models;

/// <summary>Body of <c>POST /ai/duo_workflows/workflows/:workflow_id/resume</c>.</summary>
public sealed record ResumeDuoWorkflowRequest
{
    /// <summary>Whether the human approves resuming the flow.</summary>
    public required bool HumanApproval { get; init; }

    /// <summary>An optional message accompanying the decision. At most 2000 characters.</summary>
    public string? HumanMessage { get; init; }
}