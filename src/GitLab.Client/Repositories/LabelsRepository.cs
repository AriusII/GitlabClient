using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class LabelsRepository(IGitLabApiConnection connection) : ILabelsRepository
{
    private const string LabelsSegment = "labels";

    public IAsyncEnumerable<GitLabLabel> ListAsync(ProjectId projectId, LabelListOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(LabelsSegment)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabLabelArray,
            cancellationToken);
    }

    public Task<GitLabLabel> GetAsync(ProjectId projectId, string name, LabelGetOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(LabelsSegment)
                .Escaped(name)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabLabel,
            cancellationToken);
    }

    public Task<GitLabLabel> CreateAsync(ProjectId projectId, CreateLabelRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(LabelsSegment).Build(),
            request,
            GitLabJsonContext.Default.CreateLabelRequest,
            GitLabJsonContext.Default.GitLabLabel,
            cancellationToken);
    }

    public Task<GitLabLabel> UpdateAsync(ProjectId projectId, string name, UpdateLabelRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(LabelsSegment).Escaped(name).Build(),
            request,
            GitLabJsonContext.Default.UpdateLabelRequest,
            GitLabJsonContext.Default.GitLabLabel,
            cancellationToken);
    }

    public Task DeleteAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal(LabelsSegment).Escaped(name).Build(),
            cancellationToken);
    }

    public Task PromoteAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(LabelsSegment)
                .Escaped(name)
                .Literal("promote")
                .Build(),
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabGroupLabel> ListForGroupAsync(GroupId groupId,
        GroupLabelListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(LabelsSegment)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabGroupLabelArray,
            cancellationToken);
    }

    public Task<GitLabGroupLabel> GetForGroupAsync(GroupId groupId, string name,
        GroupLabelGetOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(LabelsSegment)
                .Escaped(name)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabGroupLabel,
            cancellationToken);
    }

    public Task<GitLabGroupLabel> CreateForGroupAsync(GroupId groupId, CreateGroupLabelRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal(LabelsSegment).Build(),
            request,
            GitLabJsonContext.Default.CreateGroupLabelRequest,
            GitLabJsonContext.Default.GitLabGroupLabel,
            cancellationToken);
    }

    public Task<GitLabGroupLabel> UpdateForGroupAsync(GroupId groupId, string name,
        UpdateGroupLabelRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal(LabelsSegment).Escaped(name).Build(),
            request,
            GitLabJsonContext.Default.UpdateGroupLabelRequest,
            GitLabJsonContext.Default.GitLabGroupLabel,
            cancellationToken);
    }

    public Task<GitLabGroupLabel> UpdateForGroupByLabelIdAsync(GroupId groupId,
        UpdateGroupLabelByIdRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal(LabelsSegment).Build(),
            request,
            GitLabJsonContext.Default.UpdateGroupLabelByIdRequest,
            GitLabJsonContext.Default.GitLabGroupLabel,
            cancellationToken);
    }

    public Task DeleteForGroupAsync(GroupId groupId, string name, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups").Segment(groupId).Literal(LabelsSegment).Escaped(name).Build(),
            cancellationToken);
    }

    public Task<GitLabGroupLabel> DeleteForGroupByQueryAsync(GroupId groupId, string name,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(LabelsSegment)
                .Query("name", name)
                .Build(),
            GitLabJsonContext.Default.GitLabGroupLabel,
            cancellationToken);
    }
}