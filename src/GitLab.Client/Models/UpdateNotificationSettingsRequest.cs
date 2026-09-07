namespace GitLab.Client.Models;

/// <summary>
///     Request body shared by the three notification-settings update endpoints -
///     <c>PUT /notification_settings</c>, <c>PUT /groups/:id/notification_settings</c> and
///     <c>PUT /projects/:id/notification_settings</c>. GitLab declares almost the same field set on all
///     three, so one record covers them all; unset members are omitted rather than sent as null.
///     <para>
///         Two members are not honoured everywhere: <see cref="NotificationEmail" /> only by the
///         instance-wide endpoint, <see cref="NewEpic" /> only by the instance and group endpoints, and
///         <see cref="Approver" /> only by the instance and project endpoints. GitLab runs Grape, which
///         answers 200 and silently ignores a parameter an endpoint does not declare, so sending one to the
///         wrong scope is harmless.
///     </para>
/// </summary>
public sealed record UpdateNotificationSettingsRequest
{
    public GitLabNotificationLevel? Level { get; init; }

    /// <summary>Instance-wide endpoint only.</summary>
    public string? NotificationEmail { get; init; }

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

    /// <summary>Instance and group endpoints only.</summary>
    public bool? NewEpic { get; init; }

    public bool? ServiceAccountFailedPipeline { get; init; }

    public bool? ServiceAccountSuccessPipeline { get; init; }

    public bool? ServiceAccountFixedPipeline { get; init; }

    /// <summary>Instance and project endpoints only.</summary>
    public bool? Approver { get; init; }
}