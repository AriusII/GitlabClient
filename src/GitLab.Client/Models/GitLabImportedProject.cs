using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Models;

/// <summary>
///     The thin project shape GitLab answers an external-forge import with
///     (<c>POST /import/github</c>, <c>POST /import/bitbucket_server</c>). It identifies the project that
///     was created; poll <c>GET /projects/:id/import</c> for the import's progress.
/// </summary>
public sealed record GitLabImportedProject
{
    public required long Id { get; init; }

    public string? Name { get; init; }

    /// <summary>The namespaced path of the new project, such as <c>gitlab-org/gitlab</c>.</summary>
    public string? FullPath { get; init; }

    /// <summary>The human-readable path, such as <c>GitLab Org / GitLab</c>.</summary>
    public string? FullName { get; init; }

    /// <summary>
    ///     Where the project's refs can be listed. Kept as a <see cref="string" /> rather than a
    ///     <see cref="Uri" /> because GitLab returns an instance-relative path here, not an absolute URL.
    /// </summary>
    [SuppressMessage("Design", "CA1056",
        Justification =
            "GitLab returns an instance-relative path here, not an absolute URL, so System.Uri would "
            + "either have to be relative (which loses the type's guarantees) or fail to parse. The "
            + "absolute-URL members of this library do use Uri.")]
    public string? RefsUrl { get; init; }

    /// <summary>Whether the created project is a fork.</summary>
    public bool? Forked { get; init; }
}