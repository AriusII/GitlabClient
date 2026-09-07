using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the External status checks resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IExternalStatusChecksService), typeof(IExternalStatusChecksClient))]
internal interface IExternalStatusChecksRepository
{
    IAsyncEnumerable<GitLabExternalStatusCheck> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default);

    Task<GitLabExternalStatusCheck> CreateAsync(ProjectId projectId, CreateExternalStatusCheckRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabExternalStatusCheck> UpdateAsync(ProjectId projectId, long checkId,
        UpdateExternalStatusCheckRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, long checkId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabMergeRequestStatusCheck> ListForMergeRequestAsync(ProjectId projectId,
        long mergeRequestIid, CancellationToken cancellationToken = default);

    Task<GitLabStatusCheckResponse> SetStatusAsync(ProjectId projectId, long mergeRequestIid,
        SetStatusCheckStatusRequest request, CancellationToken cancellationToken = default);

    Task RetryAsync(ProjectId projectId, long mergeRequestIid, long externalStatusCheckId,
        CancellationToken cancellationToken = default);
}