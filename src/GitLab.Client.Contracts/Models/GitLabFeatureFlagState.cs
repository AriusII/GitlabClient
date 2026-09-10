using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>scope</c> filter of <c>GET /projects/:id/feature_flags</c>. GitLab returns both enabled and
///     disabled flags when the parameter is omitted.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabFeatureFlagState>))]
public enum GitLabFeatureFlagState
{
    /// <summary>Only flags whose <see cref="GitLabFeatureFlag.Active" /> is true.</summary>
    [JsonStringEnumMemberName("enabled")] Enabled,

    /// <summary>Only flags whose <see cref="GitLabFeatureFlag.Active" /> is false.</summary>
    [JsonStringEnumMemberName("disabled")] Disabled
}