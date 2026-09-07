using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     How a webhook's <c>push_events_branch_filter</c> is interpreted.
///     <para>
///         Only the request side is typed. GitLab's response schema declares
///         <c>branch_filter_strategy</c> as an open string, so the hook DTOs keep it as one: a strategy added
///         in a future GitLab release would otherwise turn every hook read into a deserialization failure.
///     </para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabHookBranchFilterStrategy>))]
public enum GitLabHookBranchFilterStrategy
{
    /// <summary>GitLab's default: the filter is a wildcard pattern such as <c>release/*</c>.</summary>
    [JsonStringEnumMemberName("wildcard")] Wildcard,

    /// <summary>The filter is a regular expression.</summary>
    [JsonStringEnumMemberName("regex")] Regex,

    /// <summary>The filter is ignored and the hook fires for every branch.</summary>
    [JsonStringEnumMemberName("all_branches")]
    AllBranches
}