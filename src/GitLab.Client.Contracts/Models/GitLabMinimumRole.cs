using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The lowest role allowed to perform a gated project action. The spec pins this same four-value
///     vocabulary on <c>ci_pipeline_variables_minimum_override_role</c> and
///     <c>feature_flags_minimum_role</c>.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabMinimumRole>))]
public enum GitLabMinimumRole
{
    /// <summary>Nobody may perform the action.</summary>
    [JsonStringEnumMemberName("no_one_allowed")]
    NoOneAllowed,

    /// <summary>Developer and above.</summary>
    [JsonStringEnumMemberName("developer")]
    Developer,

    /// <summary>Maintainer and above.</summary>
    [JsonStringEnumMemberName("maintainer")]
    Maintainer,

    /// <summary>Owner only.</summary>
    [JsonStringEnumMemberName("owner")] Owner
}