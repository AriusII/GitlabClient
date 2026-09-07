using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>Where one entry of <see cref="DuoChatRequest.AdditionalContext" /> came from.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<DuoChatContextCategory>))]
public enum DuoChatContextCategory
{
    /// <summary>A whole file.</summary>
    [JsonStringEnumMemberName("file")] File,

    /// <summary>An excerpt of a file.</summary>
    [JsonStringEnumMemberName("snippet")] Snippet,

    /// <summary>A merge request.</summary>
    [JsonStringEnumMemberName("merge_request")]
    MergeRequest,

    /// <summary>An issue.</summary>
    [JsonStringEnumMemberName("issue")] Issue,

    /// <summary>A declared dependency of the project.</summary>
    [JsonStringEnumMemberName("dependency")]
    Dependency,

    /// <summary>Local git state - branch, status, recent commits.</summary>
    [JsonStringEnumMemberName("local_git")]
    LocalGit,

    /// <summary>Terminal output.</summary>
    [JsonStringEnumMemberName("terminal")] Terminal,

    /// <summary>A rule the user configured for the assistant to follow.</summary>
    [JsonStringEnumMemberName("user_rule")]
    UserRule,

    /// <summary>The repository as a whole.</summary>
    [JsonStringEnumMemberName("repository")]
    Repository,

    /// <summary>A directory listing.</summary>
    [JsonStringEnumMemberName("directory")]
    Directory,

    /// <summary>The agent's own execution environment.</summary>
    [JsonStringEnumMemberName("agent_user_environment")]
    AgentUserEnvironment
}