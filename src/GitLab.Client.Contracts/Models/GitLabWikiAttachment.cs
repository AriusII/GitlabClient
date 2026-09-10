namespace GitLab.Client.Models;

/// <summary>
///     GitLab's answer to <c>POST /projects/:id/wikis/attachments</c> and
///     <c>POST /groups/:id/wikis/attachments</c>: where the uploaded file landed in the wiki's
///     repository.
/// </summary>
public sealed record GitLabWikiAttachment
{
    /// <summary>The stored file name.</summary>
    public string? FileName { get; init; }

    /// <summary>
    ///     The repository-relative path the file was committed to, under <c>uploads/</c> - for example
    ///     <c>uploads/6a061c4cf9f1c28cb22c384b4b8d4e3c/dk.png</c>.
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>The wiki branch the attachment commit landed on.</summary>
    public string? Branch { get; init; }

    /// <summary>Where the file can be fetched from, and the ready-made markdown snippet that embeds it.</summary>
    public GitLabWikiAttachmentLink? Link { get; init; }
}