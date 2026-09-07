namespace GitLab.Client.Models;

/// <summary>One group or project to write into an offline transfer export.</summary>
public sealed record OfflineExportEntityRequest
{
    /// <summary>
    ///     The entity's path on this instance - <c>source/full/path</c>, not
    ///     <c>https://example.com/source/full/path</c>.
    /// </summary>
    public required string FullPath { get; init; }
}