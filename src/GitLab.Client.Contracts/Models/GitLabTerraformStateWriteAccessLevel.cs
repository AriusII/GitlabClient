using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The minimum role a Terraform state protection rule demands before a member may write the state
///     (<c>minimum_access_level_for_write</c>).
/// </summary>
/// <remarks>
///     The rule entity echoes this back as a bare string, which is why
///     <see cref="GitLabTerraformStateProtectionRule.MinimumAccessLevelForWrite" /> stays a string: GitLab
///     declares the closed vocabulary on the request only.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabTerraformStateWriteAccessLevel>))]
public enum GitLabTerraformStateWriteAccessLevel
{
    [JsonStringEnumMemberName("developer")]
    Developer,

    [JsonStringEnumMemberName("maintainer")]
    Maintainer,

    [JsonStringEnumMemberName("owner")] Owner,

    [JsonStringEnumMemberName("admin")] Admin
}