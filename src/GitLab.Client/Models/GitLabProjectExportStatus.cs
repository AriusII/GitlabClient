using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The state of the most recent export of a project (<c>GET /projects/:id/export</c>) - the value you
///     poll after scheduling one with <c>POST /projects/:id/export</c>.
/// </summary>
public sealed record GitLabProjectExportStatus
{
    public required long Id { get; init; }

    public string? Description { get; init; }

    public string? Name { get; init; }

    public string? NameWithNamespace { get; init; }

    public string? Path { get; init; }

    public string? PathWithNamespace { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>
    ///     Where the export has got to. Only <see cref="GitLabProjectExportState.Finished" /> means the
    ///     archive can be downloaded.
    /// </summary>
    public GitLabProjectExportState? ExportStatus { get; init; }

    /// <summary>Download locations for the finished archive. Null until the export finishes.</summary>
    [JsonPropertyName("_links")]
    public GitLabProjectExportLinks? Links { get; init; }
}