using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's audit-log APIs: the instance-wide log (<c>/audit_events</c>, administrators only)
///     and a single group's log (<c>/groups/:id/audit_events</c>, group owners and administrators).
///     <para>
///         This is deliberately separate from <see cref="IProjectsClient" />'s
///         <c>ListAuditEventsAsync</c>/<c>GetAuditEventAsync</c>, which cover a project's own audit log
///         (<c>/projects/:id/audit_events</c>) - three independent route families with independent
///         access checks, sharing only the entity shape.
///     </para>
///     <para>
///         Audit records exist to answer "who did what, and when" for compliance review, so their
///         <see cref="GitLabAuditEvent.Details" /> payload can carry sensitive detail about other users'
///         actions. Treat every field here with the same care you would a credential.
///     </para>
/// </summary>
public interface IAuditEventsClient
{
    /// <summary>Streams the instance-wide audit log. Requires administrator access.</summary>
    IAsyncEnumerable<GitLabAuditEvent> ListAsync(AuditEventListOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one instance audit event by ID. Requires administrator access.</summary>
    Task<GitLabAuditEvent> GetAsync(long auditEventId, CancellationToken cancellationToken = default);

    /// <summary>Streams a group's audit log. Requires the Owner role on the group, or administrator access.</summary>
    IAsyncEnumerable<GitLabAuditEvent> ListForGroupAsync(GroupId groupId,
        GroupAuditEventListOptions? options = null, CancellationToken cancellationToken = default);
}