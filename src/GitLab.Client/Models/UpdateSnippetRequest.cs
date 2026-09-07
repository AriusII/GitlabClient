using GitLab.Client.Domain;

namespace GitLab.Client.Models;

/// <summary>
///     The body of <c>PUT /snippets/:id</c> and <c>PUT /projects/:id/snippets/:snippet_id</c>. Every
///     member is optional: unset properties are omitted from the payload rather than sent as null, so a
///     partial update leaves the rest of the snippet alone.
///     <para>
///         Updating a snippet that holds more than one file <em>must</em> go through <see cref="Files" />,
///         which is also the only way to add, rename or remove a file. <see cref="Content" /> and
///         <see cref="FileName" /> are the legacy single-file form and are mutually exclusive with it.
///     </para>
/// </summary>
public sealed record UpdateSnippetRequest
{
    public string? Title { get; init; }

    public string? Description { get; init; }

    /// <summary>Who can see the snippet.</summary>
    public GitLabVisibility? Visibility { get; init; }

    /// <summary>
    ///     The per-file actions to apply - create, update, delete or move. Mutually exclusive with
    ///     <see cref="Content" /> and <see cref="FileName" />, and mandatory for a multi-file snippet.
    /// </summary>
    public IReadOnlyList<UpdateSnippetFileRequest>? Files { get; init; }

    /// <summary>The single file's new content, in the legacy form. Mutually exclusive with <see cref="Files" />.</summary>
    public string? Content { get; init; }

    /// <summary>The single file's new name, in the legacy form. Mutually exclusive with <see cref="Files" />.</summary>
    public string? FileName { get; init; }
}