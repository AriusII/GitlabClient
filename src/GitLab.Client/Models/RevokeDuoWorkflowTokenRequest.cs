namespace GitLab.Client.Models;

/// <summary>Body of <c>POST /ai/duo_workflows/revoke_token</c>.</summary>
public sealed record RevokeDuoWorkflowTokenRequest
{
    /// <summary>The <c>ai_workflows</c>-scoped access token to revoke.</summary>
    public required string Token { get; init; }
}