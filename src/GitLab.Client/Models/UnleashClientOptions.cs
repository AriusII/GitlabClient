using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     The identifying query parameters every Unleash client sends on
///     <c>/feature_flags/unleash/:project_id</c>. GitLab records them against the project so the
///     "connected clients" view has something to show; neither is required to get an answer.
/// </summary>
[GitLabQuery]
public readonly record struct UnleashClientOptions
{
    /// <summary>The Unleash client's instance ID.</summary>
    public string? InstanceId { get; init; }

    /// <summary>The Unleash client's application name, which selects the environment the flags are resolved for.</summary>
    public string? AppName { get; init; }
}