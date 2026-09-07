using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Whether a checkpoint channel blob appends to the channel or replaces it (<c>step_action</c> on
///     <c>POST /ai/duo_workflows/workflows/:id/checkpoints</c>).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabDuoWorkflowChannelBlobStepAction>))]
public enum GitLabDuoWorkflowChannelBlobStepAction
{
    /// <summary>Append this blob to the channel's existing conversation.</summary>
    [JsonStringEnumMemberName("conversation")]
    Conversation,

    /// <summary>Replace the channel's contents with this blob.</summary>
    [JsonStringEnumMemberName("compaction")]
    Compaction
}