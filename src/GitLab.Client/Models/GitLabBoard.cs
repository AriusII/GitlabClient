namespace GitLab.Client.Models;

/// <summary>An issue board, as returned by the GitLab Boards API (<c>/projects/:id/boards</c>).</summary>
/// <remarks>
///     Four fields GitLab returns are deliberately not modelled, each because the spec's declared shape
///     cannot be trusted to deserialize:
///     <list type="bullet">
///         <item>
///             <c>labels</c> - the spec declares a single <c>APIEntitiesLabelBasic</c> object while GitLab
///             returns an array. Typing it either way risks a <see cref="System.Text.Json.JsonException" />
///             on real payloads.
///         </item>
///         <item><c>milestone</c> - a bare <c>type: object</c> with no properties; nothing can be modelled from it.</item>
///         <item>
///             <c>group</c> - <c>APIEntitiesBasicGroupDetails</c> carries only id/web_url/name and cannot
///             satisfy <see cref="GitLabGroup" />'s required Path and Visibility.
///         </item>
///         <item>
///             <c>project</c> - dropped for symmetry with <c>group</c>. The reduced project shape happens to
///             carry every member <see cref="GitLabProject" /> requires today, but depending on
///             <c>visibility</c> always being projected is the kind of assumption that becomes a
///             <see cref="System.Text.Json.JsonException" /> on a self-managed instance.
///         </item>
///     </list>
/// </remarks>
public sealed record GitLabBoard
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    /// <summary>Whether the board hides the Open (backlog) list.</summary>
    public bool? HideBacklogList { get; init; }

    /// <summary>Whether the board hides the Closed list.</summary>
    public bool? HideClosedList { get; init; }

    /// <summary>The board's columns. Present on the board endpoints; absent when a board is projected elsewhere.</summary>
    public IReadOnlyList<GitLabBoardList>? Lists { get; init; }

    /// <summary>The issue weight the board is scoped to, when the board has a weight scope (GitLab Premium).</summary>
    public int? Weight { get; init; }
}