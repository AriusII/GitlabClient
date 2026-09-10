namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>PUT /projects/:id/repository/submodules/:submodule</c>.</summary>
public sealed record UpdateSubmoduleRequest
{
    /// <summary>The commit SHA in the submodule's own repository to point at.</summary>
    public required string CommitSha { get; init; }

    /// <summary>The branch in this repository to commit the pointer change to.</summary>
    public required string Branch { get; init; }

    /// <summary>The commit message. GitLab generates one when unset.</summary>
    public string? CommitMessage { get; init; }
}