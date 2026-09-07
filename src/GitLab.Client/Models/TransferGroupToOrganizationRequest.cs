namespace GitLab.Client.Models;

/// <summary>Request body for <c>POST /groups/:id/transfer_to_organization</c>.</summary>
public sealed record TransferGroupToOrganizationRequest
{
    /// <summary>The organization to move the group into.</summary>
    public required long OrganizationId { get; init; }
}