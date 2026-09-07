namespace GitLab.Client.Models;

/// <summary>Request body for <c>POST /projects/:id/repository/branches</c>.</summary>
public sealed record CreateBranchRequest
{
    /// <summary>The name of the branch to create.</summary>
    public required string Branch { get; init; }

    /// <summary>The commit SHA, branch or tag the new branch starts from.</summary>
    public required string Ref { get; init; }
}