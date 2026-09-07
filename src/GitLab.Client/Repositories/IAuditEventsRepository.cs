using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Services;
using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Repositories;

/// <summary>
///     Raw GitLab data access for audit events: builds routes via
///     <see cref="Infrastructure.Routing.GitLabRouteBuilder" /> and calls
///     <see cref="IGitLabApiConnection" />. Knows GitLab's wire format; nothing above this layer should
///     build a route or touch <see cref="IGitLabApiConnection" /> directly.
/// </summary>
[GenerateClientLayers(typeof(IAuditEventsService), typeof(IAuditEventsClient))]
internal interface IAuditEventsRepository
{
    IAsyncEnumerable<GitLabAuditEvent> ListAsync(AuditEventListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabAuditEvent> GetAsync(long auditEventId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabAuditEvent> ListForGroupAsync(GroupId groupId,
        GroupAuditEventListOptions? options = null, CancellationToken cancellationToken = default);
}