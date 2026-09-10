using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The minimum role a <see cref="GitLabPackageProtectionRule" /> demands before a member may delete a
///     matching package (<c>minimum_access_level_for_delete</c> on the create/update request). Narrower
///     than <see cref="GitLabPackageProtectionRulePushAccessLevel" /> - GitLab does not accept
///     <c>maintainer</c> here.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabPackageProtectionRuleDeleteAccessLevel>))]
public enum GitLabPackageProtectionRuleDeleteAccessLevel
{
    [JsonStringEnumMemberName("owner")] Owner,

    [JsonStringEnumMemberName("admin")] Admin
}