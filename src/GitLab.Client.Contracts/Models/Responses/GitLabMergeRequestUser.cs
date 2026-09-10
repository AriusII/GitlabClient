namespace GitLab.Client.Models.Responses;

/// <summary>
///     The authenticated caller's permissions embedded as <c>user</c> in a merge request response.
///     This is not a <see cref="GitLabUser" />: GitLab provides a permission projection containing only
///     <c>can_merge</c>.
/// </summary>
public sealed record GitLabMergeRequestUser
{
    /// <summary>Whether the authenticated caller may merge this merge request.</summary>
    public bool? CanMerge { get; init; }
}