using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class WikisRepository(IGitLabApiConnection connection) : IWikisRepository
{
    private const string WikisSegment = "wikis";

    public IAsyncEnumerable<GitLabWikiPage> ListForProjectAsync(ProjectId projectId, bool? withContent = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(WikisSegment)
                .Query("with_content", withContent)
                .Build(),
            GitLabJsonContext.Default.GitLabWikiPageArray,
            cancellationToken);
    }

    public Task<GitLabWikiPage> GetForProjectAsync(ProjectId projectId, string slug, string? version = null,
        bool? renderHtml = null, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(WikisSegment)
                .Escaped(slug)
                .Query("version", version)
                .Query("render_html", renderHtml)
                .Build(),
            GitLabJsonContext.Default.GitLabWikiPage,
            cancellationToken);
    }

    public Task<GitLabWikiPage> CreateForProjectAsync(ProjectId projectId, CreateWikiPageRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(WikisSegment)
                .Build(),
            request,
            GitLabJsonContext.Default.CreateWikiPageRequest,
            GitLabJsonContext.Default.GitLabWikiPage,
            cancellationToken);
    }

    public Task<GitLabWikiPage> UpdateForProjectAsync(ProjectId projectId, string slug, UpdateWikiPageRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(WikisSegment)
                .Escaped(slug)
                .Build(),
            request,
            GitLabJsonContext.Default.UpdateWikiPageRequest,
            GitLabJsonContext.Default.GitLabWikiPage,
            cancellationToken);
    }

    public Task DeleteForProjectAsync(ProjectId projectId, string slug, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(WikisSegment)
                .Escaped(slug)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabWikiAttachment> UploadAttachmentForProjectAsync(ProjectId projectId, GitLabFileUpload file,
        string? branch = null, CancellationToken cancellationToken = default)
    {
        return connection.PostFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(WikisSegment).Literal("attachments")
                .Build(),
            file,
            BranchFormField(branch),
            GitLabJsonContext.Default.GitLabWikiAttachment,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabWikiPage> ListForGroupAsync(GroupId groupId, bool? withContent = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(WikisSegment)
                .Query("with_content", withContent)
                .Build(),
            GitLabJsonContext.Default.GitLabWikiPageArray,
            cancellationToken);
    }

    public Task<GitLabWikiPage> GetForGroupAsync(GroupId groupId, string slug, string? version = null,
        bool? renderHtml = null, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(WikisSegment)
                .Escaped(slug)
                .Query("version", version)
                .Query("render_html", renderHtml)
                .Build(),
            GitLabJsonContext.Default.GitLabWikiPage,
            cancellationToken);
    }

    public Task<GitLabWikiPage> CreateForGroupAsync(GroupId groupId, CreateWikiPageRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(WikisSegment)
                .Build(),
            request,
            GitLabJsonContext.Default.CreateWikiPageRequest,
            GitLabJsonContext.Default.GitLabWikiPage,
            cancellationToken);
    }

    public Task<GitLabWikiPage> UpdateForGroupAsync(GroupId groupId, string slug, UpdateWikiPageRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(WikisSegment)
                .Escaped(slug)
                .Build(),
            request,
            GitLabJsonContext.Default.UpdateWikiPageRequest,
            GitLabJsonContext.Default.GitLabWikiPage,
            cancellationToken);
    }

    public Task DeleteForGroupAsync(GroupId groupId, string slug, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(WikisSegment)
                .Escaped(slug)
                .Build(),
            cancellationToken);
    }

    public Task<GitLabWikiAttachment> UploadAttachmentForGroupAsync(GroupId groupId, GitLabFileUpload file,
        string? branch = null, CancellationToken cancellationToken = default)
    {
        return connection.PostFileAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal(WikisSegment).Literal("attachments")
                .Build(),
            file,
            BranchFormField(branch),
            GitLabJsonContext.Default.GitLabWikiAttachment,
            cancellationToken);
    }

    private static Dictionary<string, string>? BranchFormField(string? branch)
    {
        return branch is null ? null : new Dictionary<string, string>(StringComparer.Ordinal) { ["branch"] = branch };
    }
}