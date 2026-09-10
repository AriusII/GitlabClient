namespace GitLab.Client.Models.Requests;

/// <summary>Moves an issue association relative to another issue association in an epic.</summary>
public sealed record ReorderEpicIssueRequest
{
    /// <summary>The association id that this issue should precede.</summary>
    public long? MoveBeforeId { get; init; }

    /// <summary>The association id that this issue should follow.</summary>
    public long? MoveAfterId { get; init; }

    /// <summary>Optional page selection accepted by GitLab's relation-reorder endpoint.</summary>
    public int? Page { get; init; }

    public int? PerPage { get; init; }
}