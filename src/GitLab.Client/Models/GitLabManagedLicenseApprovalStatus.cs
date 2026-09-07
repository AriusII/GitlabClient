using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The verdict a project records against a software licence, as accepted by
///     <c>POST</c> and <c>PATCH /projects/:id/managed_licenses</c>.
/// </summary>
/// <remarks>
///     This is the write vocabulary only. GitLab renamed the pair from <c>approved</c>/<c>blacklisted</c>
///     and still returns the old spellings for policies created before the rename, which is why
///     <see cref="GitLabManagedLicense.ApprovalStatus" /> stays a string.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabManagedLicenseApprovalStatus>))]
public enum GitLabManagedLicenseApprovalStatus
{
    /// <summary>The licence may be used by the project.</summary>
    [JsonStringEnumMemberName("allowed")] Allowed,

    /// <summary>The licence is not permitted, and a merge request introducing it is flagged.</summary>
    [JsonStringEnumMemberName("denied")] Denied
}