namespace GitLab.Client.Models;

/// <summary>
///     A Git blob addressed by its own SHA (<c>GET /projects/:id/repository/blobs/:sha</c>).
///     <see cref="Content" /> is Base64-encoded; use the raw sibling to stream the bytes instead.
/// </summary>
public sealed record GitLabBlob
{
    /// <summary>The blob's SHA.</summary>
    public string? Sha { get; init; }

    /// <summary>The blob's size in bytes.</summary>
    public long? Size { get; init; }

    /// <summary>How <see cref="Content" /> is encoded, normally <c>base64</c>.</summary>
    public string? Encoding { get; init; }

    /// <summary>The blob's contents, encoded per <see cref="Encoding" />.</summary>
    public string? Content { get; init; }
}