using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Where a Terraform state protection rule accepts writes from (<c>allowed_from</c>). GitLab defaults a
///     new rule to <see cref="Anywhere" />.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabTerraformStateAllowedFrom>))]
public enum GitLabTerraformStateAllowedFrom
{
    /// <summary>Any authenticated caller with the required role, CI or not.</summary>
    [JsonStringEnumMemberName("anywhere")] Anywhere,

    /// <summary>Only a CI job, using its <c>CI_JOB_TOKEN</c>.</summary>
    [JsonStringEnumMemberName("ci_only")] CiOnly,

    /// <summary>Only a CI job running against a protected branch.</summary>
    [JsonStringEnumMemberName("ci_on_protected_branch_only")]
    CiOnProtectedBranchOnly
}