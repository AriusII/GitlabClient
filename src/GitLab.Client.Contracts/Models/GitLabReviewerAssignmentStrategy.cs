using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>How GitLab picks reviewers for a new merge request (<c>reviewer_assignment_strategy</c>).</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabReviewerAssignmentStrategy>))]
public enum GitLabReviewerAssignmentStrategy
{
    /// <summary>Reviewers are assigned by hand.</summary>
    [JsonStringEnumMemberName("disabled")] Disabled,

    /// <summary>Reviewers are taken from the matching CODEOWNERS entries.</summary>
    [JsonStringEnumMemberName("code_owners")]
    CodeOwners
}