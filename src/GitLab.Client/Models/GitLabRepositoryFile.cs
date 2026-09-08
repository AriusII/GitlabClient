namespace GitLab.Client.Models;

/// <summary>A repository file's metadata and (base64-encoded) content, as returned by the RepositoryFiles API.</summary>
public sealed record GitLabRepositoryFile
{
    public required string FileName { get; init; }

    public required string FilePath { get; init; }

    public long? Size { get; init; }

    public string? Encoding { get; init; }

    /// <summary>The SHA-256 of the file's raw (decoded) content, independent of <see cref="Encoding" />.</summary>
    public string? ContentSha256 { get; init; }

    public string? Content { get; init; }

    public string? Ref { get; init; }

    public string? BlobId { get; init; }

    public string? CommitId { get; init; }

    public string? LastCommitId { get; init; }

    /// <summary>True when the file's Git mode has the executable bit set.</summary>
    public bool? ExecuteFilemode { get; init; }
}