namespace GitLab.Client.Models;

/// <summary>
///     One file in a project's markdown attachment store, as listed by
///     <c>GET /projects/:id/uploads</c>. Listing requires the Maintainer or Owner role.
///     <para>
///         This is the administrative view of an upload - it identifies the file but carries no link to it.
///         The pastable markdown snippet only ever comes back from the upload call itself, as
///         <see cref="GitLabProjectUploadLink" />.
///     </para>
/// </summary>
public sealed record GitLabProjectUpload
{
    /// <summary>The upload's numeric id, which addresses it in the <c>/uploads/:upload_id</c> routes.</summary>
    public required long Id { get; init; }

    /// <summary>The stored size in bytes.</summary>
    public long? Size { get; init; }

    /// <summary>The file name the uploader sent. GitLab stores it verbatim, so it can contain spaces.</summary>
    public required string Filename { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>Who uploaded the file. Null for uploads GitLab cannot attribute to a live user any more.</summary>
    public GitLabUploadAuthor? UploadedBy { get; init; }
}