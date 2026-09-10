using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The kind of asset a <see cref="GitLabReleaseLink" /> points at. GitLab enumerates this one closed
///     set on both the create and the update request, so it is modelled as an enum rather than a string;
///     it defaults to <see cref="Other" /> when the parameter is omitted.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabReleaseLinkType>))]
public enum GitLabReleaseLinkType
{
    [JsonStringEnumMemberName("other")] Other,

    [JsonStringEnumMemberName("runbook")] Runbook,

    [JsonStringEnumMemberName("image")] Image,

    [JsonStringEnumMemberName("package")] Package
}