using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>The project's default answer to whether commits are squashed on merge (<c>squash_option</c>).</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabSquashOption>))]
public enum GitLabSquashOption
{
    /// <summary>Squashing is not offered.</summary>
    [JsonStringEnumMemberName("never")] Never,

    /// <summary>Every merge request is squashed, with no way to opt out.</summary>
    [JsonStringEnumMemberName("always")] Always,

    /// <summary>Offered and checked by default.</summary>
    [JsonStringEnumMemberName("default_on")]
    DefaultOn,

    /// <summary>Offered and unchecked by default.</summary>
    [JsonStringEnumMemberName("default_off")]
    DefaultOff
}