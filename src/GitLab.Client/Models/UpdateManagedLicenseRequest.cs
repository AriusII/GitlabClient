namespace GitLab.Client.Models;

/// <summary>
///     The body of <c>PATCH /projects/:id/managed_licenses/:managed_license_id</c>. Both members are
///     optional - GitLab leaves anything omitted untouched.
/// </summary>
public sealed record UpdateManagedLicenseRequest
{
    /// <summary>The new licence name, or <see langword="null" /> to leave it as it is.</summary>
    public string? Name { get; init; }

    /// <summary>The new verdict, or <see langword="null" /> to leave it as it is.</summary>
    public GitLabManagedLicenseApprovalStatus? ApprovalStatus { get; init; }
}