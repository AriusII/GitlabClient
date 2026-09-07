namespace GitLab.Client.Models;

/// <summary>
///     GitLab's answer to <c>POST /projects/:id/uploads</c>: where the file landed, and the markdown that
///     embeds it in an issue, merge request or comment.
/// </summary>
public sealed record GitLabProjectUploadLink
{
    public required long Id { get; init; }

    /// <summary>The alt text GitLab derived from the file name, used inside <see cref="Markdown" />.</summary>
    public string? Alt { get; init; }

    /// <summary>
    ///     The project-relative location of the file - <c>/uploads/&lt;secret&gt;/&lt;filename&gt;</c>. A
    ///     relative reference, not an absolute URL: resolve it against the project's web URL to fetch it
    ///     from a browser, or pass the secret and file name to
    ///     <c>IProjectUploadsClient.DownloadBySecretAsync</c> to fetch it through the API.
    /// </summary>
    public Uri? Url { get; init; }

    /// <summary>The instance-relative path including the project - <c>/-/project/:id/uploads/...</c>.</summary>
    public string? FullPath { get; init; }

    /// <summary>The ready-made markdown snippet, <c>![alt](url)</c> for an image and <c>[alt](url)</c> otherwise.</summary>
    public string? Markdown { get; init; }
}