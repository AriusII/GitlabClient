using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Models;

/// <summary>
///     The richer project shape returned by the external-forge import endpoints that also report progress
///     (<c>POST /import/bitbucket</c>, <c>POST /import/github/cancel</c>). A superset of
///     <see cref="GitLabImportedProject" />.
/// </summary>
public sealed record GitLabRemoteImportedProject
{
    public required long Id { get; init; }

    public string? Name { get; init; }

    /// <summary>The namespaced path of the project, such as <c>gitlab-org/gitlab</c>.</summary>
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

    /// <summary>Whether the project is a fork.</summary>
    public bool? Forked { get; init; }

    /// <summary>The repository on the source forge the project was imported from, such as <c>source/source-repo</c>.</summary>
    public string? ImportSource { get; init; }

    /// <summary>How far along the import is.</summary>
    public GitLabRemoteImportState? ImportStatus { get; init; }

    /// <summary>GitLab's own display name for <see cref="ImportStatus" />, already localised.</summary>
    public string? HumanImportStatusName { get; init; }

    /// <summary>
    ///     A link back to the source repository. Kept as a <see cref="string" />: GitLab returns a relative
    ///     path (<c>/source/source-repo</c>) for some providers and an absolute URL for others.
    /// </summary>
    public string? ProviderLink { get; init; }

    /// <summary>The failure message when <see cref="ImportStatus" /> is <see cref="GitLabRemoteImportState.Failed" />.</summary>
    public string? ImportError { get; init; }

    /// <summary>A non-fatal warning raised during the import, such as a partially imported relation.</summary>
    public string? ImportWarning { get; init; }

    /// <summary>How the project relates to the source - <c>owned</c>, <c>collaborated</c> or <c>organization</c>.</summary>
    public string? RelationType { get; init; }
}