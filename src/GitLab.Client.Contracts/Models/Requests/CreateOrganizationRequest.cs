namespace GitLab.Client.Models.Requests;

/// <summary>
///     The fields of <c>POST /organizations</c> that travel outside the avatar's own multipart part. See
///     <see cref="Abstractions.IOrganizationsClient.CreateAsync" /> for how the avatar itself is passed.
/// </summary>
public sealed record CreateOrganizationRequest
{
    public required string Name { get; init; }

    public required string Path { get; init; }

    public string? Description { get; init; }

    public GitLabOrganizationVisibility? Visibility { get; init; }
}