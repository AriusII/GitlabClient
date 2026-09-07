namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /projects/:id/milestones/:milestone_id</c> and
///     <c>PUT /groups/:id/milestones/:milestone_id</c>. Every member is optional; GitLab requires at least one.
/// </summary>
public sealed record UpdateMilestoneRequest
{
    public string? Title { get; init; }

    /// <summary>
    ///     The state transition to apply: <c>"close"</c> or <c>"activate"</c>. A string rather than an enum
    ///     because it is a verb applied to the milestone, not the <c>state</c> the entity reports back.
    /// </summary>
    public string? StateEvent { get; init; }

    public string? Description { get; init; }

    /// <summary>Sent as an ISO 8601 date (<c>yyyy-MM-dd</c>), which is the only form GitLab accepts here.</summary>
    public DateOnly? DueDate { get; init; }

    /// <summary>Sent as an ISO 8601 date (<c>yyyy-MM-dd</c>), which is the only form GitLab accepts here.</summary>
    public DateOnly? StartDate { get; init; }
}