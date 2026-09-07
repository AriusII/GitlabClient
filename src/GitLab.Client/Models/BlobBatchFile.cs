namespace GitLab.Client.Models;

/// <summary>One entry of a <see cref="BlobBatchRequest" />: a path to read at a ref.</summary>
public sealed record BlobBatchFile
{
    /// <summary>Full path from the repository root, for example <c>src/App.cs</c>.</summary>
    public required string Path { get; init; }

    /// <summary>The branch, tag or commit SHA to read <see cref="Path" /> at.</summary>
    public required string Ref { get; init; }
}