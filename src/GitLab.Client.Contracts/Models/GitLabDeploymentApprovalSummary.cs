namespace GitLab.Client.Models;

/// <summary>
///     The protected-environment approval information embedded in an extended deployment response.
/// </summary>
public sealed record GitLabDeploymentApprovalSummary
{
    /// <summary>
    ///     The approval rules GitLab reports for the protected environment. GitLab's runtime response is an array,
    ///     despite the singular schema declaration in the pinned OpenAPI document.
    /// </summary>
    public IReadOnlyList<GitLabDeploymentApprovalRule>? Rules { get; init; }
}