using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Where a Duo flow was started from (<c>POST /ai/duo_workflows/workflows</c>,
///     <c>POST /ai/duo_workflows/agent_workflows</c>).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabDuoWorkflowEnvironment>))]
public enum GitLabDuoWorkflowEnvironment
{
    /// <summary>Started from an IDE extension.</summary>
    [JsonStringEnumMemberName("ide")] Ide,

    /// <summary>Started from the GitLab web UI.</summary>
    [JsonStringEnumMemberName("web")] Web,

    /// <summary>Started from a partial chat surface.</summary>
    [JsonStringEnumMemberName("chat_partial")]
    ChatPartial,

    /// <summary>Started from Duo Chat.</summary>
    [JsonStringEnumMemberName("chat")] Chat,

    /// <summary>Started without a user present, by GitLab itself.</summary>
    [JsonStringEnumMemberName("ambient")] Ambient,

    /// <summary>Started by a third-party client outside GitLab.</summary>
    [JsonStringEnumMemberName("external")] External
}