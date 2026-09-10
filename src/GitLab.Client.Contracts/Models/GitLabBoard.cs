using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     An issue-board response (<c>APIEntitiesBoard</c>). All members are nullable because GitLab 19.4
///     defines no required board response fields and embeds boards at more than one width.
/// </summary>
public sealed record GitLabBoard
{
    public long? Id { get; init; }

    public string? Name { get; init; }

    public bool? HideBacklogList { get; init; }

    public bool? HideClosedList { get; init; }

    public GitLabBasicProjectDetails? Project { get; init; }

    public IReadOnlyList<GitLabBoardList>? Lists { get; init; }

    public GitLabBasicGroupDetails? Group { get; init; }

    /// <summary>
    ///     The board milestone is a bare <c>object</c> in the GitLab 19.4 schema, with no declared
    ///     properties. It remains raw JSON rather than an invented DTO.
    /// </summary>
    public JsonElement? Milestone { get; init; }

    public GitLabBasicUser? Assignee { get; init; }

    /// <summary>
    ///     The specification declares one <c>APIEntitiesLabelBasic</c> object here. It is deliberately
    ///     not widened to an array without a versioned OpenAPI contract change.
    /// </summary>
    public GitLabBasicLabel? Labels { get; init; }

    public int? Weight { get; init; }
}