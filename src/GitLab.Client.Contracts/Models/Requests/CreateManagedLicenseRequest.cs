namespace GitLab.Client.Models.Requests;

/// <summary>The body of <c>POST /projects/:id/managed_licenses</c>.</summary>
public sealed record CreateManagedLicenseRequest
{
    /// <summary>The licence name, such as <c>MIT</c>. GitLab matches it verbatim against scan results.</summary>
    public required string Name { get; init; }

    /// <summary>The verdict to record for the licence.</summary>
    public required GitLabManagedLicenseApprovalStatus ApprovalStatus { get; init; }
}