using GitLab.Client.Models.Requests;

namespace GitLab.Client.Models;

/// <summary>
///     A protected tag (or wildcard pattern such as <c>release/*</c>) on a project, as returned by the
///     GitLab Protected Tags API.
/// </summary>
public sealed record GitLabProtectedTag
{
    public required string Name { get; init; }

    /// <summary>
    ///     Plural on the way out, singular on the way in: GitLab accepts a scalar
    ///     <c>create_access_level</c> on <see cref="ProtectTagRequest" /> and answers with this richer array,
    ///     which can also identify an individual user, group, deploy key, or custom member role.
    /// </summary>
    public IReadOnlyList<GitLabProtectedTagAccessLevel>? CreateAccessLevels { get; init; }
}