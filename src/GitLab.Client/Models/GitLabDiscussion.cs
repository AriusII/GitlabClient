namespace GitLab.Client.Models;

/// <summary>
///     A discussion thread on an issue, merge request or commit, as returned by the GitLab Discussions API
///     (<c>/projects/:id/:noteable/:id/discussions</c>). A discussion is a container for one or more
///     <see cref="GitLabNote" />s; a single stand-alone comment comes back as a discussion whose
///     <see cref="IndividualNote" /> is <see langword="true" />.
/// </summary>
public sealed record GitLabDiscussion
{
    /// <summary>GitLab's discussion identifier - a 40-character hex string, not a number.</summary>
    public required string Id { get; init; }

    /// <summary>
    ///     <see langword="true" /> when this is a stand-alone comment rather than a resolvable thread;
    ///     replies cannot be added to an individual note.
    /// </summary>
    public bool? IndividualNote { get; init; }

    public bool? Resolvable { get; init; }

    public bool? Resolved { get; init; }

    public IReadOnlyList<GitLabNote>? Notes { get; init; }
}