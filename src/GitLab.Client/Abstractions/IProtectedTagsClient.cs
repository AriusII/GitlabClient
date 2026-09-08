using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>Wraps the GitLab "Protected tags" API area (<c>/projects/:id/protected_tags</c>).</summary>
public interface IProtectedTagsClient
{
    /// <summary>Lists a project's protected tags and wildcard patterns.</summary>
    IAsyncEnumerable<GitLabProtectedTag> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves one protected tag of a project by name or wildcard pattern.</summary>
    Task<GitLabProtectedTag> GetAsync(ProjectId projectId, string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Protects a tag, or several at once through a wildcard pattern. GitLab accepts a scalar
    ///     <see cref="ProtectTagRequest.CreateAccessLevel" /> here and answers with the
    ///     <see cref="GitLabProtectedTag.CreateAccessLevels" /> array.
    /// </summary>
    Task<GitLabProtectedTag> ProtectAsync(ProjectId projectId, ProtectTagRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a project's protection from a tag or wildcard pattern.</summary>
    Task UnprotectAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default);
}