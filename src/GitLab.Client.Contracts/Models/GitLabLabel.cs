namespace GitLab.Client.Models;

/// <summary>
///     A label on a GitLab project, as returned by the project Labels API. Group labels are
///     <see cref="GitLabGroupLabel" />: they carry neither <see cref="Priority" /> nor
///     <see cref="IsProjectLabel" />.
/// </summary>
public sealed record GitLabLabel
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public string? Description { get; init; }

    /// <summary>The description rendered to HTML by GitLab's Markdown pipeline.</summary>
    public string? DescriptionHtml { get; init; }

    public string? Color { get; init; }

    public string? TextColor { get; init; }

    public bool? Archived { get; init; }

    public int? OpenIssuesCount { get; init; }

    public int? ClosedIssuesCount { get; init; }

    public int? OpenMergeRequestsCount { get; init; }

    public int? Priority { get; init; }

    /// <summary>
    ///     <c>false</c> when the label reaching a project listing is actually inherited from an ancestor
    ///     group rather than defined on the project itself.
    /// </summary>
    public bool? IsProjectLabel { get; init; }

    /// <summary>Whether the authenticated user is subscribed to this label.</summary>
    public bool? Subscribed { get; init; }
}