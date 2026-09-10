using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Delivery state of a flow event (<c>PUT /ai/duo_workflows/workflows/:id/events/:event_id</c>).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabDuoWorkflowEventStatus>))]
public enum GitLabDuoWorkflowEventStatus
{
    /// <summary>Recorded but not yet handed to the flow.</summary>
    [JsonStringEnumMemberName("queued")] Queued,

    /// <summary>Handed to the flow.</summary>
    [JsonStringEnumMemberName("delivered")]
    Delivered
}