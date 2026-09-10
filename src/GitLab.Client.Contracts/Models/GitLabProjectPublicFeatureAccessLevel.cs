using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The four-value form of <see cref="GitLabProjectFeatureAccessLevel" />, used by the two features
///     the spec lets a project expose to anonymous visitors: <c>pages_access_level</c> and
///     <c>package_registry_access_level</c>.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabProjectPublicFeatureAccessLevel>))]
public enum GitLabProjectPublicFeatureAccessLevel
{
    /// <summary>The feature is turned off for everyone.</summary>
    [JsonStringEnumMemberName("disabled")] Disabled,

    /// <summary>Only project members can see the feature.</summary>
    [JsonStringEnumMemberName("private")] Private,

    /// <summary>Everyone who can see the project can see the feature.</summary>
    [JsonStringEnumMemberName("enabled")] Enabled,

    /// <summary>Anyone can see the feature, even on a project that is not itself public.</summary>
    [JsonStringEnumMemberName("public")] Public
}