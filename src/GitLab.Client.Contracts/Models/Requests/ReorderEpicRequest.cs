namespace GitLab.Client.Models.Requests;

/// <summary>Moves a child epic relative to one of its sibling epics.</summary>
public sealed record ReorderEpicRequest
{
    public long? MoveBeforeId { get; init; }

    public long? MoveAfterId { get; init; }
}