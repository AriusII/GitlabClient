using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>The <c>sort</c> vocabulary of the merge request listings. GitLab defaults to descending.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<MergeRequestSortDirection>))]
public enum MergeRequestSortDirection
{
    [JsonStringEnumMemberName("asc")] Ascending,

    [JsonStringEnumMemberName("desc")] Descending
}