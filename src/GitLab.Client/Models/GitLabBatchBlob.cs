namespace GitLab.Client.Models;

/// <summary>
///     One file's contents from a batch blob read (<c>POST /projects/:id/repository/blobs/batch</c>).
/// </summary>
public sealed record GitLabBatchBlob
{
    /// <summary>The path that was requested.</summary>
    public required string Path { get; init; }

    /// <summary>The ref the path was read at.</summary>
    public string? Ref { get; init; }

    /// <summary>The file's size in bytes, before any truncation.</summary>
    public long? Size { get; init; }

    /// <summary>
    ///     True when the file exceeded GitLab's 1 MB per-blob cap and <see cref="Content" /> is cut short.
    /// </summary>
    public bool? Truncated { get; init; }

    /// <summary>How <see cref="Content" /> is encoded, normally <c>base64</c>.</summary>
    public string? Encoding { get; init; }

    /// <summary>The file's contents, encoded per <see cref="Encoding" />.</summary>
    public string? Content { get; init; }
}