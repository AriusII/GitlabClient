using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>Which Git transport protocols an instance permits.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabGitAccessProtocol>))]
public enum GitLabGitAccessProtocol
{
    [JsonStringEnumMemberName("ssh")] Ssh,

    [JsonStringEnumMemberName("http")] Http,

    [JsonStringEnumMemberName("all")] All
}