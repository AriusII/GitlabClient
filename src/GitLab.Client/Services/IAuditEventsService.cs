using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Services;

/// <summary>
///     Business-orchestration layer for audit events, sitting between the public
///     <c>IAuditEventsClient</c> controller and <c>IAuditEventsRepository</c>'s raw GitLab access.
///     Mirrors the repository's method shapes 1:1 today (its implementation is generated); this is the
///     seam where request validation, caching, or cross-resource composition would go once the resource
///     needs more than pass-through.
/// </summary>
internal interface IAuditEventsService
{
    IAsyncEnumerable<GitLabAuditEvent> ListAsync(AuditEventListOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<GitLabAuditEvent> GetAsync(long auditEventId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<GitLabAuditEvent> ListForGroupAsync(GroupId groupId,
        GroupAuditEventListOptions? options = null, CancellationToken cancellationToken = default);
}