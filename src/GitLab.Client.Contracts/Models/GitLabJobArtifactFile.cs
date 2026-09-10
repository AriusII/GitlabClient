namespace GitLab.Client.Models;

/// <summary>
///     The aggregate artifact archive attached directly to a job - the wire's
///     <c>APIEntitiesCiJobArtifactFile</c> projection.
/// </summary>
public sealed record GitLabJobArtifactFile
{
    public string? Filename { get; init; }

    /// <summary>Archive size in bytes.</summary>
    public long? Size { get; init; }
}