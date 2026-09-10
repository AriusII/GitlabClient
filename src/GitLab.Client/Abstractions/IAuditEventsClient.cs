using GitLab.Client.Domain;
using GitLab.Client.Models;
using GitLab.Client.Query;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's audit-log APIs: the instance-wide log (<c>/audit_events</c>, administrators only),
///     group logs (<c>/groups/:id/audit_events</c>), and project logs
///     (<c>/projects/:id/audit_events</c>).
///     <para>
///         The three route families have independent access checks but share GitLab's
///         <c>APIEntitiesAuditEvent</c> response shape. This focused client keeps compliance/audit callers
///         on one coherent API surface.
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
    /// <param name="options">The required entity category and optional entity/date/paging filters.</param>
    /// <param name="cancellationToken">Cancels the enumeration between pages.</param>
    IAsyncEnumerable<GitLabAuditEvent> ListAsync(AuditEventListOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one instance audit event by ID. Requires administrator access.</summary>
    /// <param name="auditEventId">The numeric audit-event ID.</param>
    /// <param name="cancellationToken">Cancels the HTTP operation.</param>
    Task<GitLabAuditEvent> GetAsync(long auditEventId, CancellationToken cancellationToken = default);

    /// <summary>Streams a group's audit log. Requires the Owner role on the group, or administrator access.</summary>
    /// <param name="groupId">The group's numeric ID or namespaced path.</param>
    /// <param name="options">Optional date and paging filters.</param>
    /// <param name="cancellationToken">Cancels the enumeration between pages.</param>
    IAsyncEnumerable<GitLabAuditEvent> ListForGroupAsync(GroupId groupId,
        GroupAuditEventListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Gets one audit event from a group. Requires the Owner role on the group, or administrator access.</summary>
    /// <param name="groupId">The group's numeric ID or namespaced path.</param>
    /// <param name="auditEventId">The numeric audit-event ID.</param>
    /// <param name="cancellationToken">Cancels the HTTP operation.</param>
    Task<GitLabAuditEvent> GetForGroupAsync(GroupId groupId, long auditEventId,
        CancellationToken cancellationToken = default);

    /// <summary>Streams a project's audit log. Requires at least the Developer role on the project.</summary>
    /// <param name="projectId">The project's numeric ID or namespaced path.</param>
    /// <param name="options">Optional date and paging filters.</param>
    /// <param name="cancellationToken">Cancels the enumeration between pages.</param>
    IAsyncEnumerable<GitLabAuditEvent> ListForProjectAsync(ProjectId projectId,
        ProjectAuditEventListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Gets one audit event from a project. Requires at least the Developer role on the project.</summary>
    /// <param name="projectId">The project's numeric ID or namespaced path.</param>
    /// <param name="auditEventId">The numeric audit-event ID.</param>
    /// <param name="cancellationToken">Cancels the HTTP operation.</param>
    Task<GitLabAuditEvent> GetForProjectAsync(ProjectId projectId, long auditEventId,
        CancellationToken cancellationToken = default);
}