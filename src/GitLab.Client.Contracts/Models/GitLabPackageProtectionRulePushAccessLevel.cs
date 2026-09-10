using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The minimum role a <see cref="GitLabPackageProtectionRule" /> demands before a member may push a
///     matching package (<c>minimum_access_level_for_push</c> on the create/update request).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabPackageProtectionRulePushAccessLevel>))]
public enum GitLabPackageProtectionRulePushAccessLevel
{
    [JsonStringEnumMemberName("maintainer")]
    Maintainer,

    [JsonStringEnumMemberName("owner")] Owner,

    [JsonStringEnumMemberName("admin")] Admin
}