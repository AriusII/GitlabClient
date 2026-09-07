namespace GitLab.Client.Models;

/// <summary>
///     A label owned by a group (<c>/groups/:id/labels</c>).
///     <para>
///         Deliberately a separate type from <see cref="GitLabLabel" />: GitLab's <c>GroupLabel</c> entity
///         carries neither <c>priority</c> nor <c>is_project_label</c>, because both are project-scoped
///         concepts. Sharing one DTO would advertise two members that a group response never fills in.
///     </para>
/// </summary>
public sealed record GitLabGroupLabel
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public string? Description { get; init; }

    /// <summary>The description rendered to HTML by GitLab's Markdown pipeline.</summary>
    public string? DescriptionHtml { get; init; }

    /// <summary>Six-digit hex notation with a leading <c>#</c>, for example <c>#FF0000</c>.</summary>
    public string? Color { get; init; }

    /// <summary>The foreground colour GitLab picked to stay readable on <see cref="Color" />.</summary>
    public string? TextColor { get; init; }

    public bool? Archived { get; init; }

    /// <summary>Only present when the request asked for <c>with_counts</c>.</summary>
    public int? OpenIssuesCount { get; init; }

    /// <summary>Only present when the request asked for <c>with_counts</c>.</summary>
    public int? ClosedIssuesCount { get; init; }

    /// <summary>Only present when the request asked for <c>with_counts</c>.</summary>
    public int? OpenMergeRequestsCount { get; init; }

    /// <summary>Whether the authenticated user is subscribed to this label.</summary>
    public bool? Subscribed { get; init; }
}