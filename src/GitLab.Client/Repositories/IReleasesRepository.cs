using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Releases resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
///     <para>
///         Every <c>tagName</c> is caller-supplied free text that routinely contains <c>/</c> and <c>.</c>
///         (<c>v1.0/rc1</c>), so it goes through <c>Escaped</c>, never <c>Literal</c>.
///     </para>
/// </summary>
[GenerateClientLayers(typeof(IReleasesService), typeof(IReleasesClient))]
internal interface IReleasesRepository
{
    IAsyncEnumerable<GitLabRelease> ListAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabRelease> ListForGroupAsync(GroupId groupId, GroupReleaseListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabRelease> GetAsync(ProjectId projectId, string tagName, CancellationToken cancellationToken = default);

    Task<GitLabRelease> CreateAsync(ProjectId projectId, CreateReleaseRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabRelease> UpdateAsync(ProjectId projectId, string tagName, UpdateReleaseRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(ProjectId projectId, string tagName, CancellationToken cancellationToken = default);

    Task<GitLabRelease> GenerateEvidenceAsync(ProjectId projectId, string tagName,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabReleaseLink> ListLinksAsync(ProjectId projectId, string tagName,
        ReleaseLinkListOptions? options = null, CancellationToken cancellationToken = default);

    Task<GitLabReleaseLink> GetLinkAsync(ProjectId projectId, string tagName, long linkId,
        CancellationToken cancellationToken = default);

    Task<GitLabReleaseLink> CreateLinkAsync(ProjectId projectId, string tagName, CreateReleaseLinkRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabReleaseLink> UpdateLinkAsync(ProjectId projectId, string tagName, long linkId,
        UpdateReleaseLinkRequest request, CancellationToken cancellationToken = default);

    Task DeleteLinkAsync(ProjectId projectId, string tagName, long linkId,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetLatestReleaseAsync(ProjectId projectId, CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> GetLatestReleaseSuffixPathAsync(ProjectId projectId, string suffixPath,
        CancellationToken cancellationToken = default);

    Task<GitLabFileResponse> DownloadReleaseAssetAsync(ProjectId projectId, string tagName, string directAssetPath,
        CancellationToken cancellationToken = default);
}