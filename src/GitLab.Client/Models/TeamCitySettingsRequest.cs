using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /projects/:id/services/teamcity</c> - GitLab's older path spelling for
///     the JetBrains TeamCity integration (run a TeamCity build configuration as the project's CI).
/// </summary>
public sealed record TeamCitySettingsRequest
{
    /// <summary>Enable SSL verification. Defaults to <c>true</c> (enabled).</summary>
    public bool? EnableSslVerification { get; init; }

    /// <summary>TeamCity root URL (for example, <c>https://teamcity.example.com</c>).</summary>
    [JsonPropertyName("teamcity_url")]
    public required Uri TeamCityUrl { get; init; }

    /// <summary>The build configuration ID of the TeamCity project.</summary>
    public required string BuildType { get; init; }

    /// <summary>A user with permissions to trigger a manual build.</summary>
    public required string Username { get; init; }

    /// <summary>The password of the user.</summary>
    public required string Password { get; init; }

    /// <summary>Trigger event for pushes to the repository.</summary>
    public bool? PushEvents { get; init; }

    /// <summary>Trigger event when a merge request is created, updated, or merged.</summary>
    public bool? MergeRequestsEvents { get; init; }

    /// <summary>Indicates whether to inherit the default settings. Defaults to <c>false</c>.</summary>
    public bool? UseInheritedSettings { get; init; }
}