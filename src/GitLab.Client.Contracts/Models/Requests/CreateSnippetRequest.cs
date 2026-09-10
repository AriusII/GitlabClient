using GitLab.Client.Domain;

namespace GitLab.Client.Models.Requests;

/// <summary>
///     The body of <c>POST /snippets</c> and <c>POST /projects/:id/snippets</c>. GitLab declares the two
///     bodies identically, so one type serves both.
///     <para>
///         <see cref="Files" /> is the form GitLab prefers: a snippet is a git repository, and only the
///         <c>files</c> array can create one with more than a single file. <see cref="Content" /> plus
///         <see cref="FileName" /> is the legacy single-file form, still accepted and still what
///         <c>file_name</c> reports back. The two are mutually exclusive - sending both is a
///         <c>400</c>.
///     </para>
/// </summary>
public sealed record CreateSnippetRequest
{
    /// <summary>The snippet's title. The one field GitLab requires in every form of this request.</summary>
    public required string Title { get; init; }

    public string? Description { get; init; }

    /// <summary>
    ///     Who can see the snippet. Required by the project endpoint; the personal endpoint defaults it to
    ///     <see cref="GitLabVisibility.Internal" />.
    /// </summary>
    public GitLabVisibility? Visibility { get; init; }

    /// <summary>
    ///     The files to create, one entry per file. Mutually exclusive with <see cref="Content" />, and the
    ///     only way to create a snippet holding more than one file.
    /// </summary>
    public IReadOnlyList<CreateSnippetFileRequest>? Files { get; init; }

    /// <summary>
    ///     The single file's content, in the legacy form. Mutually exclusive with <see cref="Files" />;
    ///     pair it with <see cref="FileName" />.
    /// </summary>
    public string? Content { get; init; }

    /// <summary>The single file's name, in the legacy form. Pair it with <see cref="Content" />.</summary>
    public string? FileName { get; init; }
}