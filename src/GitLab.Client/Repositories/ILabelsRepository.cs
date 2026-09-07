using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Labels resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(ILabelsService), typeof(ILabelsClient))]
internal interface ILabelsRepository
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