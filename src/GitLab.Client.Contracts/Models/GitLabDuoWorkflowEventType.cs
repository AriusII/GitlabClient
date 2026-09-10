using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>The kind of event posted to a running flow (<c>POST /ai/duo_workflows/workflows/:id/events</c>).</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabDuoWorkflowEventType>))]
public enum GitLabDuoWorkflowEventType
{
    /// <summary>Suspend the flow at the next checkpoint.</summary>
    [JsonStringEnumMemberName("pause")] Pause,

    /// <summary>Continue a suspended flow.</summary>
    [JsonStringEnumMemberName("resume")] Resume,

    /// <summary>Terminate the flow.</summary>
    [JsonStringEnumMemberName("stop")] Stop,

    /// <summary>A message from the human to the agent.</summary>
    [JsonStringEnumMemberName("message")] Message,

    /// <summary>A human answer to a question the agent asked.</summary>
    [JsonStringEnumMemberName("response")] Response,

    /// <summary>The agent is asking the human for input.</summary>
    [JsonStringEnumMemberName("require_input")]
    RequireInput
}