using System.Text.Json;

namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /jobs/request</c> - a GitLab Runner asking for the next job to execute.
///     This is the runner registration protocol GitLab Runner itself speaks; an application client has
///     little reason to call it unless it is implementing a custom runner.
/// </summary>
public sealed record JobRequestRequest
{
    /// <summary>The runner's authentication token.</summary>
    public required string Token { get; init; }

    /// <summary>The runner's system identifier.</summary>
    public string? SystemId { get; init; }

    /// <summary>The runner's queue <c>last_update</c> token, echoed back to support long-polling.</summary>
    public string? LastUpdate { get; init; }

    /// <summary>
    ///     Free-form runner metadata (version, platform, architecture, feature flags). The spec types it
    ///     as an untyped object with no fixed shape, so it stays a raw <see cref="JsonElement" /> rather
    ///     than a shape this library would have to invent.
    /// </summary>
    public JsonElement? Info { get; init; }

    /// <summary>The runner's session data, for jobs that expose an interactive web terminal.</summary>
    public JsonElement? Session { get; init; }
}