using System.Text.Json.Serialization;

using GitLab.Client.Models.Requests;

namespace GitLab.Client.Models;

/// <summary>
///     How visible one project feature is - the value behind the whole <c>*_access_level</c> family on
///     <see cref="CreateProjectRequest" /> and <see cref="UpdateProjectRequest" /> (issues, repository,
///     merge requests, wiki, builds, snippets, releases, environments, ...).
/// </summary>
/// <remarks>
///     Modelled as an enum rather than a string because the spec enumerates the vocabulary on every one
///     of those parameters and GitLab stores it as an integer-backed database enum, so it is genuinely
///     closed. The two features that also accept <c>public</c> use
///     <see cref="GitLabProjectPublicFeatureAccessLevel" /> instead.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabProjectFeatureAccessLevel>))]
public enum GitLabProjectFeatureAccessLevel
{
    /// <summary>The feature is turned off for everyone.</summary>
    [JsonStringEnumMemberName("disabled")] Disabled,

    /// <summary>Only project members can see the feature.</summary>
    [JsonStringEnumMemberName("private")] Private,

    /// <summary>Everyone who can see the project can see the feature.</summary>
    [JsonStringEnumMemberName("enabled")] Enabled
}