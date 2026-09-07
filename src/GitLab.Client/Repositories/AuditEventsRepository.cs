using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class AuditEventsRepository(IGitLabApiConnection connection) : IAuditEventsRepository
{
    private const string Root = "audit_events";

    public IAsyncEnumerable<GitLabAuditEvent> ListAsync(AuditEventListOptions? options = null,
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
}