using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     The acknowledgement every write on the Jira Connect / GitLab for Jira (Forge) surface answers
///     with - the spec's <c>APIEntitiesBasicSuccess</c>. It carries no identifier for what was created
///     and no representation of it; reaching this type without an exception is the result.
/// </summary>
public sealed record GitLabJiraConnectResult
{
    /// <summary>
    ///     GitLab's success marker. The spec types it as an untyped object while the Grape endpoints behind
    ///     it render a bare <c>true</c>, so it is captured as a raw <see cref="JsonElement" /> rather than as
    ///     a <c>bool</c> or a record: either shape deserializes, and a future payload that carries detail
    ///     here stays readable instead of becoming a <see cref="JsonException" />.
    /// </summary>
    public JsonElement? Success { get; init; }
}