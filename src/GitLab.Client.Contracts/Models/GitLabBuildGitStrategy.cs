using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>How a runner fetches the repository for a job (<c>build_git_strategy</c>).</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabBuildGitStrategy>))]
public enum GitLabBuildGitStrategy
{
    /// <summary>Reuse the existing working copy and fetch the new refs into it. GitLab's default.</summary>
    [JsonStringEnumMemberName("fetch")] Fetch,

    /// <summary>Clone the repository from scratch for every job.</summary>
    [JsonStringEnumMemberName("clone")] Clone
}