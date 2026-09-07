using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>Wraps the GitLab "Protected tags" API area (<c>/projects/:id/protected_tags</c>).</summary>
public interface IProtectedTagsClient
{
    IAsyncEnumerable<GitLabProtectedTag> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabProtectedTag> GetAsync(ProjectId projectId, string name,
        CancellationToken cancellationToken = default);

    Task<GitLabProtectedTag> ProtectAsync(ProjectId projectId, ProtectTagRequest request,
        CancellationToken cancellationToken = default);

    Task UnprotectAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default);
}