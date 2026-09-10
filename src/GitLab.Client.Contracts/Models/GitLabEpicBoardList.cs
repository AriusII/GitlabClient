namespace GitLab.Client.Models;

/// <summary>A column on a read-only legacy epic board.</summary>
public sealed record GitLabEpicBoardList
{
    public long? Id { get; init; }

    public GitLabBasicLabel? Label { get; init; }

    public int? Position { get; init; }

    /// <summary>For example, <c>backlog</c>, <c>closed</c>, or <c>label</c>.</summary>
    public string? ListType { get; init; }
}