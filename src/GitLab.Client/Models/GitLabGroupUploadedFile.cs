namespace GitLab.Client.Models;

/// <summary>
///     What <c>POST /groups/:id/uploads</c> answers with: where the stored file now lives, plus the
///     markdown snippet that embeds it in a description or comment.
/// </summary>
public sealed record GitLabGroupUploadedFile
{
    public long? Id { get; init; }

    /// <summary>Alt text GitLab derived from the file name.</summary>
    public string? Alt { get; init; }

    /// <summary>Instance-relative location of the file, so a relative URI rather than an absolute one.</summary>
    public Uri? Url { get; init; }

    /// <summary>The same location prefixed with the group's path.</summary>
    public string? FullPath { get; init; }

    /// <summary>Ready-made markdown that renders the file, for example <c>![alt](/uploads/.../file.png)</c>.</summary>
    public string? Markdown { get; init; }
}