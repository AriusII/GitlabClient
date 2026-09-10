using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>Review state GitLab can record when publishing draft merge-request notes.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabReviewerState>))]
public enum GitLabReviewerState
{
    [JsonStringEnumMemberName("requested_changes")]
    RequestedChanges,

    [JsonStringEnumMemberName("reviewed")] Reviewed
}