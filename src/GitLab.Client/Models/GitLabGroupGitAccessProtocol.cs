using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The Git transports a group's projects may be cloned over (<c>enabled_git_access_protocol</c>).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabGroupGitAccessProtocol>))]
public enum GitLabGroupGitAccessProtocol
{
    [JsonStringEnumMemberName("ssh")] Ssh,

    [JsonStringEnumMemberName("http")] Http,

    /// <summary>Both SSH and HTTP.</summary>
    [JsonStringEnumMemberName("all")] All
}