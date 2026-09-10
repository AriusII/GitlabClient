namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PUT /projects/:id/issues/:issue_iid/reorder</c>, which moves an issue within a
///     manually ordered board or list. Supply at least one of the two neighbours.
/// </summary>
public sealed record ReorderIssueRequest
{
    /// <summary>The id of the issue this one should end up after.</summary>
    public long? MoveAfterId { get; init; }

    /// <summary>The id of the issue this one should end up before.</summary>
    public long? MoveBeforeId { get; init; }
}