namespace GitLab.Client.Models;

/// <summary>Body of <c>POST /ai/duo_workflows/workflows/:id/events</c>.</summary>
public sealed record CreateDuoWorkflowEventRequest
{
    /// <summary>What the event asks the flow to do.</summary>
    public required GitLabDuoWorkflowEventType EventType { get; init; }

    /// <summary>
    ///     The message from the human. GitLab declares the field mandatory, so send at least an empty
    ///     string for the control events (<c>pause</c>, <c>resume</c>, <c>stop</c>) that carry no text.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>A UUID for correlating this event across GitLab and the flow service.</summary>
    public string? CorrelationId { get; init; }
}