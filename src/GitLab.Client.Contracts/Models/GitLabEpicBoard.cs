namespace GitLab.Client.Models;

/// <summary>A read-only group epic board returned by GitLab's legacy Epics API.</summary>
public sealed record GitLabEpicBoard
{
    public long? Id { get; init; }

    public string? Name { get; init; }

    public bool? HideBacklogList { get; init; }

    public bool? HideClosedList { get; init; }

    public GitLabBasicGroupDetails? Group { get; init; }

    public IReadOnlyList<GitLabBasicLabel>? Labels { get; init; }

    public IReadOnlyList<GitLabEpicBoardList>? Lists { get; init; }
}