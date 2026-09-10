using System.Text.Json.Serialization;

using GitLab.Client.Models.Responses;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>state</c> filter of the merge request listings. This is the query vocabulary, which GitLab
///     closes: <see cref="GitLabMergeRequest.State" /> stays a bare string, because a state GitLab adds
///     later must not turn a healthy response into a <c>JsonException</c>.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<MergeRequestStateFilter>))]
public enum MergeRequestStateFilter
{
    [JsonStringEnumMemberName("opened")] Opened,

    [JsonStringEnumMemberName("closed")] Closed,

    [JsonStringEnumMemberName("locked")] Locked,

    [JsonStringEnumMemberName("merged")] Merged,

    [JsonStringEnumMemberName("all")] All
}