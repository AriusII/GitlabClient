using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The order in which a project's resource groups release waiting jobs
///     (<c>resource_group_default_process_mode</c>).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabResourceGroupProcessMode>))]
public enum GitLabResourceGroupProcessMode
{
    /// <summary>No ordering guarantee - whichever job is picked up first runs first.</summary>
    [JsonStringEnumMemberName("unordered")]
    Unordered,

    /// <summary>The oldest waiting job runs next.</summary>
    [JsonStringEnumMemberName("oldest_first")]
    OldestFirst,

    /// <summary>The newest waiting job runs next.</summary>
    [JsonStringEnumMemberName("newest_first")]
    NewestFirst,

    /// <summary>The newest job that is actually ready to run goes next.</summary>
    [JsonStringEnumMemberName("newest_ready_first")]
    NewestReadyFirst
}