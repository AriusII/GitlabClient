using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The publication status GitLab assigns a generic package file - <see cref="Default" /> makes it
///     discoverable normally, <see cref="Hidden" /> excludes it from the package list UI and most listing
///     endpoints without deleting it.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabPackageFileStatus>))]
public enum GitLabPackageFileStatus
{
    [JsonStringEnumMemberName("default")] Default,

    [JsonStringEnumMemberName("hidden")] Hidden
}