using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class AuditEventsClient(IGitLabApiConnection connection) : IAuditEventsClient
{
    private const string Root = "audit_events";

    public IAsyncEnumerable<GitLabAuditEvent> ListAsync(AuditEventListOptions options,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create(Root)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabAuditEventArray,
            cancellationToken);
    }

    public Task<GitLabAuditEvent> GetAsync(long auditEventId, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create(Root)
                .Segment(auditEventId)
                .Build(),
            GitLabJsonContext.Default.GitLabAuditEvent,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabAuditEvent> ListForGroupAsync(GroupId groupId,
        GroupAuditEventListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(Root)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabAuditEventArray,
            cancellationToken);
    }

    public Task<GitLabAuditEvent> GetForGroupAsync(GroupId groupId, long auditEventId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("groups")
                .Segment(groupId)
                .Literal(Root)
                .Segment(auditEventId)
                .Build(),
            GitLabJsonContext.Default.GitLabAuditEvent,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabAuditEvent> ListForProjectAsync(ProjectId projectId,
        ProjectAuditEventListOptions? options = null, CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(Root)
                .QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.GitLabAuditEventArray,
            cancellationToken);
    }

    public Task<GitLabAuditEvent> GetForProjectAsync(ProjectId projectId, long auditEventId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal(Root)
                .Segment(auditEventId)
                .Build(),
            GitLabJsonContext.Default.GitLabAuditEvent,
            cancellationToken);
    }
}