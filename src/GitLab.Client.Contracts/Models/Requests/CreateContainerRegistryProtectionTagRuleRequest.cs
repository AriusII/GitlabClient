namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body of <c>POST /projects/:id/registry/protection/tag/rules</c>. Unlike the repository-level
///     <see cref="CreateContainerRegistryProtectionRuleRequest" />, GitLab requires both access levels
///     up front - there is no unrestricted default for tags.
/// </summary>
public sealed record CreateContainerRegistryProtectionTagRuleRequest
{
    /// <summary>
    ///     The container tag name pattern to protect, e.g. <c>v*-release</c>. The wildcard character
    ///     <c>*</c> is allowed.
    /// </summary>
    public required string TagNamePattern { get; init; }

    /// <summary>The minimum role required to push a matching tag.</summary>
    public required GitLabContainerRegistryProtectionAccessLevel MinimumAccessLevelForPush { get; init; }

    /// <summary>The minimum role required to delete a matching tag.</summary>
    public required GitLabContainerRegistryProtectionAccessLevel MinimumAccessLevelForDelete { get; init; }
}