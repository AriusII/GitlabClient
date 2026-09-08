using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /jobs/:id</c> - a GitLab Runner reporting a job's state back to GitLab.
///     Part of the runner protocol; see <see cref="JobRequestRequest" />.
/// </summary>
public sealed record UpdateJobStateRequest
{
    /// <summary>The job's authentication token.</summary>
    public required string Token { get; init; }

    /// <summary>
    ///     The job's status. The spec types this as a bare string with no enumerated vocabulary rather
    ///     than pinning it to the <c>running</c>/<c>success</c>/<c>failed</c> values its description
    ///     names, so it stays a string here too: a value GitLab Runner adds later must not turn a healthy
    ///     request into a compile-time-safe but runtime-impossible one.
    /// </summary>
    public string? State { get; init; }

    /// <summary>The job's trace CRC32 checksum, for GitLab to verify against what it has stored.</summary>
    public string? Checksum { get; init; }

    /// <summary>The reason the job failed, when <see cref="State" /> is <c>failed</c>.</summary>
    public string? FailureReason { get; init; }

    /// <summary>Free-form build-log state. The spec types it as an untyped object.</summary>
    public JsonElement? Output { get; init; }

    /// <summary>The job's process exit code.</summary>
    public int? ExitCode { get; init; }
}