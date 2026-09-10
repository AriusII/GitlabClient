using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>How strictly the project requires merge requests to go through a merge train.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabMergeTrainEnforcement>))]
public enum GitLabMergeTrainEnforcement
{
    /// <summary>The train can be bypassed.</summary>
    [JsonStringEnumMemberName("allow_bypass")]
    AllowBypass,

    /// <summary>Everyone must use the train.</summary>
    [JsonStringEnumMemberName("enforce_for_all_users")]
    EnforceForAllUsers,

    /// <summary>Everyone must use the train except owners, who may override it.</summary>
    [JsonStringEnumMemberName("enforce_with_owner_override")]
    EnforceWithOwnerOverride
}