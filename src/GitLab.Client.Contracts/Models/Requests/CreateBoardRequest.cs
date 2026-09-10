namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>POST /projects/:id/boards</c>. The board name is the only accepted field.</summary>
public sealed record CreateBoardRequest
{
    public required string Name { get; init; }
}