namespace GitLab.Client.Models;

/// <summary>
///     Body of <c>PATCH /groups/:id/enterprise_users/:user_id</c>. Every member is optional; an unset
///     member is omitted from the request and left unchanged by GitLab.
/// </summary>
public sealed record UpdateEnterpriseUserRequest
{
    public string? Name { get; init; }

    public string? Email { get; init; }

    public int? ProjectsLimit { get; init; }

    public bool? CanCreateGroup { get; init; }
}