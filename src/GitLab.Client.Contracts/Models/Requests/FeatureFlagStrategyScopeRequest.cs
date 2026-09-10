using System.Text.Json.Serialization;

namespace GitLab.Client.Models.Requests;

/// <summary>
///     One entry of the <c>scopes</c> array on <see cref="FeatureFlagStrategyRequest" /> - the
///     environments a strategy applies to.
/// </summary>
public sealed record FeatureFlagStrategyScopeRequest
{
    /// <summary>The environment name or wildcard - <c>*</c>, <c>production</c>, <c>review/*</c>.</summary>
    public string? EnvironmentScope { get; init; }

    /// <summary>The ID of the existing scope to edit. Update only.</summary>
    public long? Id { get; init; }

    /// <summary>
    ///     Set to <c>true</c>, together with <see cref="Id" />, to delete the scope. Update only. The wire
    ///     name is spelled out because the snake_case policy would emit <c>destroy</c> and the entry would
    ///     silently survive.
    /// </summary>
    [JsonPropertyName("_destroy")]
    public bool? Destroy { get; init; }
}