namespace GitLab.Client.Models.Requests;

/// <summary>Body of <c>PUT /ai/duo_workflows/workflows/:id/events/:event_id</c>.</summary>
public sealed record UpdateDuoWorkflowEventRequest
{
    /// <summary>The delivery state to move the event to.</summary>
    public required GitLabDuoWorkflowEventStatus EventStatus { get; init; }
}