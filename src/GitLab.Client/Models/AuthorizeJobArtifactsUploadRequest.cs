namespace GitLab.Client.Models;

/// <summary>Request body for <c>POST /jobs/:id/artifacts/authorize</c>, the Workhorse pre-upload check.</summary>
public sealed record AuthorizeJobArtifactsUploadRequest
{
    /// <summary>The job's authentication token.</summary>
    public string? Token { get; init; }

    /// <summary>The size, in bytes, of the artifact about to be uploaded.</summary>
    public long? Filesize { get; init; }

    /// <summary>The type of artifact being authorized.</summary>
    public GitLabJobArtifactUploadType? ArtifactType { get; init; }
}