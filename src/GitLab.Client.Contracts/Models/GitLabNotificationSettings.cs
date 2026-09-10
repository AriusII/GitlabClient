using GitLab.Client.Models.Requests;

namespace GitLab.Client.Models;

/// <summary>
///     A notification-settings snapshot, as returned by the GitLab Notification settings API
///     (<c>/notification_settings</c>, <c>/groups/:id/notification_settings</c>,
///     <c>/projects/:id/notification_settings</c>). The same shape covers all three scopes.
///     <para>
///         GitLab's OpenAPI schema for this response only declares <c>level</c> (and, for the instance-wide
///         endpoint, <c>events</c> and <c>notification_email</c>) - the per-event toggle flags below are exposed by a
///         conditional block in GitLab's entity that grape-swagger cannot describe, so they are absent from
///         the generated schema even though GitLab does send them whenever <see cref="Level" /> is
///         <see cref="GitLabNotificationLevel.Custom" />. They are modelled here from the shape GitLab's own
///         update endpoints declare (see <see cref="UpdateNotificationSettingsRequest" />), which the read
///         and write sides mirror. Every flag is nullable, and an unrecognised property in the response is
///         silently skipped, so this carries no deserialization risk if a future GitLab release changes the
///         set.
///     </para>
/// </summary>
public sealed record GitLabNotificationSettings
{
    public GitLabNotificationLevel? Level { get; init; }

    /// <summary>
    ///     GitLab's event summary as exposed by the notification-settings response schema. This is a
    ///     server-defined string, rather than the individual custom-mode toggles below.
    /// </summary>
    public string? Events { get; init; }

    /// <summary>The email address notifications are sent to. Returned only by the instance-wide endpoint.</summary>
    public string? NotificationEmail { get; init; }

    /// <summary>
    ///     Below: the individual event toggles, populated only when <see cref="Level" /> is
    ///     <see cref="GitLabNotificationLevel.Custom" />. <see cref="NewEpic" /> is honoured on the instance
    ///     and group scopes only; <see cref="Approver" /> on the instance and project scopes only.
    /// </summary>
    public bool? NewRelease { get; init; }

    public bool? NewNote { get; init; }

    public bool? NewIssue { get; init; }

    public bool? ReopenIssue { get; init; }

    public bool? CloseIssue { get; init; }

    public bool? ReassignIssue { get; init; }

    public bool? IssueDue { get; init; }

    public bool? NewMergeRequest { get; init; }

    public bool? PushToMergeRequest { get; init; }

    public bool? ReopenMergeRequest { get; init; }

    public bool? CloseMergeRequest { get; init; }

    public bool? ReassignMergeRequest { get; init; }

    public bool? ChangeReviewerMergeRequest { get; init; }

    public bool? MergeMergeRequest { get; init; }

    public bool? FailedPipeline { get; init; }

    public bool? FixedPipeline { get; init; }

    public bool? SuccessPipeline { get; init; }

    public bool? MovedProject { get; init; }

    public bool? MergeWhenPipelineSucceeds { get; init; }

    /// <summary>Instance and group scopes only.</summary>
    public bool? NewEpic { get; init; }

    public bool? ServiceAccountFailedPipeline { get; init; }

    public bool? ServiceAccountSuccessPipeline { get; init; }

    public bool? ServiceAccountFixedPipeline { get; init; }

    /// <summary>Instance and project scopes only.</summary>
    public bool? Approver { get; init; }
}