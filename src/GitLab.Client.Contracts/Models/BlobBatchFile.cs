using GitLab.Client.Models.Requests;

namespace GitLab.Client.Models;

/// <summary>One entry of a <see cref="BlobBatchRequest" />: a path to read at a ref.</summary>
public sealed record BlobBatchFile
{
    /// <summary>Full path from the repository root, for example <c>src/App.cs</c>.</summary>
    public required string Path { get; init; }

    /// <summary>
    ///     The branch, tag or commit SHA to read <see cref="Path" /> at. When omitted, GitLab reads the
    ///     project's default branch. The OpenAPI contract makes this field optional; keeping that distinction
    ///     avoids forcing callers to make an extra default-branch lookup before they can use the batch API.
    /// </summary>
    public string? Ref { get; init; }
}