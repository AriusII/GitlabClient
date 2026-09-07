using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Whether GitLab cancels a branch's still-pending pipelines when a newer commit is pushed
///     (<c>auto_cancel_pending_pipelines</c>). Spelled as a two-value string on the wire rather than a
///     boolean, which is why it is its own type.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabAutoCancelPendingPipelines>))]
public enum GitLabAutoCancelPendingPipelines
{
    /// <summary>Superseded pipelines keep running.</summary>
    [JsonStringEnumMemberName("disabled")] Disabled,

    /// <summary>Superseded pipelines are cancelled.</summary>
    [JsonStringEnumMemberName("enabled")] Enabled
}