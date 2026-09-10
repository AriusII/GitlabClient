using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The minimum role required to push or delete a matching container image or tag, on the
///     <em>create</em> request for a <see cref="GitLabContainerRegistryProtectionRule" /> or a
///     <see cref="GitLabContainerRegistryProtectionTagRule" />. The update request instead uses
///     <see cref="GitLabContainerRegistryProtectionAccessLevelOrUnset" />, which additionally allows
///     clearing the restriction.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabContainerRegistryProtectionAccessLevel>))]
public enum GitLabContainerRegistryProtectionAccessLevel
{
    [JsonStringEnumMemberName("maintainer")]
    Maintainer,

    [JsonStringEnumMemberName("owner")] Owner,

    [JsonStringEnumMemberName("admin")] Admin
}