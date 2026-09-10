namespace GitLab.Client.Models;

/// <summary>
///     The <c>link</c> member of <see cref="GitLabWikiAttachment" /> - where the uploaded file can be
///     fetched from, and the markdown that embeds it.
/// </summary>
public sealed record GitLabWikiAttachmentLink
{
    /// <summary>The wiki-relative path to the file, for example <c>uploads/6a061c4cf9f1c28cb22c384b4b8d4e3c/dk.png</c>.</summary>
    public Uri? Url { get; init; }

    /// <summary>The ready-made markdown snippet, <c>![name](url)</c> for an image and <c>[name](url)</c> otherwise.</summary>
    public string? Markdown { get; init; }
}