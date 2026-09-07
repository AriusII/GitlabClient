namespace GitLab.Client.Abstractions;

/// <summary>
///     The file part of a <c>multipart/form-data</c> upload - a project or group upload, a wiki attachment,
///     an issue metric image, a secure file, an avatar or a project import archive.
/// </summary>
/// <remarks>
///     The stream is BORROWED, never owned: the transport reads it and leaves it open, so the caller's
///     <c>await using</c> keeps meaning what it says and a rewindable stream stays reusable for a retry.
/// </remarks>
public sealed record GitLabFileUpload
{
    /// <summary>
    ///     The form field name nearly every GitLab upload endpoint expects, and the default for
    ///     <see cref="FieldName" />.
    /// </summary>
    public const string DefaultFieldName = "file";

    /// <summary>
    ///     The bytes to upload. Read once, from its current position, and deliberately not disposed - the
    ///     caller keeps ownership.
    /// </summary>
    public required Stream Content { get; init; }

    /// <summary>The file name sent in the part's <c>Content-Disposition</c> header. GitLab stores it verbatim.</summary>
    public required string FileName { get; init; }

    /// <summary>
    ///     The media type of the part (<c>image/png</c>, <c>application/gzip</c>). Left off the wire when
    ///     null, which makes the server fall back to <c>application/octet-stream</c>.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    ///     The form field name to send the file under. Only the handful of endpoints that name it something
    ///     other than <see cref="DefaultFieldName" /> - <c>avatar</c>, for one - need to set this.
    /// </summary>
    public string FieldName { get; init; } = DefaultFieldName;
}