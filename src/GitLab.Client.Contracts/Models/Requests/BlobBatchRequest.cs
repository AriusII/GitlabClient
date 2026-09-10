namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /projects/:id/repository/blobs/batch</c>, which reads several files in one
///     round trip. GitLab caps the batch at 20 files and truncates each blob at 1 MB.
/// </summary>
public sealed record BlobBatchRequest
{
    /// <summary>The files to read, at most 20.</summary>
    public required IReadOnlyList<BlobBatchFile> Files { get; init; }
}