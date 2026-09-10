using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The <c>.info</c> document of the Go module proxy protocol
///     (<c>GET /projects/:id/packages/go/:module_name/@v/:module_version.info</c>) - see
///     <c>go help goproxy</c>.
/// </summary>
/// <remarks>
///     Both members are capitalized on the wire, which is the Go proxy protocol's own convention rather
///     than GitLab's usual snake_case, hence the explicit <see cref="JsonPropertyNameAttribute" />s.
///     <see cref="Time" /> stays a plain string: GitLab documents it as an RFC 3339 timestamp in practice,
///     but the spec types it as a bare string with no declared format.
/// </remarks>
public sealed record GitLabGoModuleVersionInfo
{
    [JsonPropertyName("Version")] public required string Version { get; init; }

    [JsonPropertyName("Time")] public string? Time { get; init; }
}