using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>One Flipper gate on a <see cref="GitLabFeature" /> - what the feature is switched on for.</summary>
public sealed record GitLabFeatureGate
{
    /// <summary>
    ///     The gate kind - <c>boolean</c>, <c>percentage_of_actors</c>, <c>percentage_of_time</c>,
    ///     <c>actors</c> or <c>groups</c>.
    /// </summary>
    public string? Key { get; init; }

    /// <summary>
    ///     The gate's value, whose JSON type follows <see cref="Key" />: a boolean for <c>boolean</c>, a
    ///     number for either percentage gate, an array of actor strings for <c>actors</c>. The spec types it
    ///     as an integer, which is only true of the percentage gates, so it is surfaced as a raw
    ///     <see cref="JsonElement" /> rather than a type that would fail to read a healthy response.
    /// </summary>
    public JsonElement? Value { get; init; }
}