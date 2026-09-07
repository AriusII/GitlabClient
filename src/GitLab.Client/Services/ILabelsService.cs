using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for Labels, sitting between the public <c>ILabelsClient</c>
///     controller and <c>ILabelsRepository</c>'s raw GitLab access. Mirrors the repository's method
///     shapes 1:1 today (its implementation is generated); this is the seam where request validation,
///     caching, or cross-resource composition would go once the resource needs more than pass-through.
/// </summary>
internal interface ILabelsService
{
    IAsyncEnumerable<GitLabLabel> ListAsync(ProjectId projectId, LabelListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabLabel> GetAsync(ProjectId projectId, string name, LabelGetOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabLabel> CreateAsync(ProjectId projectId, CreateLabelRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabLabel> UpdateAsync(ProjectId projectId, string name, UpdateLabelRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default);

    Task PromoteAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabGroupLabel> ListForGroupAsync(GroupId groupId, GroupLabelListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabGroupLabel> GetForGroupAsync(GroupId groupId, string name, GroupLabelGetOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabGroupLabel> CreateForGroupAsync(GroupId groupId, CreateGroupLabelRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabGroupLabel> UpdateForGroupAsync(GroupId groupId, string name, UpdateGroupLabelRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabGroupLabel> UpdateForGroupByLabelIdAsync(GroupId groupId, UpdateGroupLabelByIdRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForGroupAsync(GroupId groupId, string name, CancellationToken cancellationToken = default);

    Task<GitLabGroupLabel> DeleteForGroupByQueryAsync(GroupId groupId, string name,
        CancellationToken cancellationToken = default);
}