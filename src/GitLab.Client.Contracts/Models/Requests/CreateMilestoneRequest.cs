namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /projects/:id/milestones</c> and <c>POST /groups/:id/milestones</c>. GitLab
///     declares one schema for both, so one type serves both routes.
/// </summary>
public sealed record CreateMilestoneRequest
{
    public required string Title { get; init; }

    public string? Description { get; init; }

    /// <summary>Sent as an ISO 8601 date (<c>yyyy-MM-dd</c>), which is the only form GitLab accepts here.</summary>
    public DateOnly? DueDate { get; init; }

    /// <summary>Sent as an ISO 8601 date (<c>yyyy-MM-dd</c>), which is the only form GitLab accepts here.</summary>
    public DateOnly? StartDate { get; init; }
}