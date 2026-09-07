using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for the Wikis resource: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" />
///     and calls <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this
///     layer should build a route or touch <see cref="IGitLabApiConnection" /> directly.
///     <para>
///         Project and group wikis are separate GitLab route families with identical shapes, so the
///         methods come in <c>...ForProjectAsync</c> / <c>...ForGroupAsync</c> pairs rather than being
///         collapsed behind one union-typed id.
///     </para>
/// </summary>
[GenerateClientLayers(typeof(IWikisService), typeof(IWikisClient))]
internal interface IWikisRepository
{
    IAsyncEnumerable<GitLabWikiPage> ListForProjectAsync(ProjectId projectId, bool? withContent = null,
        CancellationToken cancellationToken = default);

    Task<GitLabWikiPage> GetForProjectAsync(ProjectId projectId, string slug, string? version = null,
        bool? renderHtml = null, CancellationToken cancellationToken = default);

    Task<GitLabWikiPage> CreateForProjectAsync(ProjectId projectId, CreateWikiPageRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabWikiPage> UpdateForProjectAsync(ProjectId projectId, string slug, UpdateWikiPageRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForProjectAsync(ProjectId projectId, string slug, CancellationToken cancellationToken = default);

    Task<GitLabWikiAttachment> UploadAttachmentForProjectAsync(ProjectId projectId, GitLabFileUpload file,
        string? branch = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabWikiPage> ListForGroupAsync(GroupId groupId, bool? withContent = null,
        CancellationToken cancellationToken = default);

    Task<GitLabWikiPage> GetForGroupAsync(GroupId groupId, string slug, string? version = null,
        bool? renderHtml = null, CancellationToken cancellationToken = default);

    Task<GitLabWikiPage> CreateForGroupAsync(GroupId groupId, CreateWikiPageRequest request,
        CancellationToken cancellationToken = default);

    Task<GitLabWikiPage> UpdateForGroupAsync(GroupId groupId, string slug, UpdateWikiPageRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteForGroupAsync(GroupId groupId, string slug, CancellationToken cancellationToken = default);

    Task<GitLabWikiAttachment> UploadAttachmentForGroupAsync(GroupId groupId, GitLabFileUpload file,
        string? branch = null, CancellationToken cancellationToken = default);
}