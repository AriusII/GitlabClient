using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>How an external agent session reached GitLab (<c>sync_type</c>).</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabAgentSyncType>))]
public enum GitLabAgentSyncType
{
    /// <summary>Reported automatically by an agent lifecycle hook.</summary>
    [JsonStringEnumMemberName("hook")] Hook,

    /// <summary>Reported by the agent's fallback path, after a hook failed to fire.</summary>
    [JsonStringEnumMemberName("fallback")] Fallback,

    /// <summary>Recorded by hand.</summary>
    [JsonStringEnumMemberName("manual")] Manual
}