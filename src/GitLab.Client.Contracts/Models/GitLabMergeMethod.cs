using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>How merge requests are merged into the target branch (<c>merge_method</c>).</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabMergeMethod>))]
public enum GitLabMergeMethod
{
    /// <summary>Fast-forward only - no merge commit, and the merge is refused unless the branch is rebased.</summary>
    [JsonStringEnumMemberName("ff")] FastForward,

    /// <summary>Rebase the source branch first, then create a merge commit.</summary>
    [JsonStringEnumMemberName("rebase_merge")]
    RebaseMerge,

    /// <summary>Always create a merge commit. GitLab's default.</summary>
    [JsonStringEnumMemberName("merge")] Merge
}