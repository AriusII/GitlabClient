namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /projects/:id/boards/:board_id/lists/:list_id</c>. The endpoint updates a
///     list's <b>position only</b> - GitLab accepts no other field here, so this type deliberately carries
///     just the one.
/// </summary>
public sealed record UpdateBoardListRequest
{
    /// <summary>The list's new one-based position on the board.</summary>
    public required int Position { get; init; }
}