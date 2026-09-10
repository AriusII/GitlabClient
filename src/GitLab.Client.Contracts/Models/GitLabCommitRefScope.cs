using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>type</c> filter of <c>GET /projects/:id/repository/commits/:sha/refs</c>. GitLab defaults
///     to <see cref="All" /> when the parameter is omitted.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabCommitRefScope>))]
public enum GitLabCommitRefScope
{
    /// <summary>Only branches that contain the commit.</summary>
    [JsonStringEnumMemberName("branch")] Branch,

    /// <summary>Only tags that point at the commit.</summary>
    [JsonStringEnumMemberName("tag")] Tag,

    /// <summary>Branches and tags together.</summary>
    [JsonStringEnumMemberName("all")] All
}