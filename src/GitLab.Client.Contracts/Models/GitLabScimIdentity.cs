namespace GitLab.Client.Models;

/// <summary>
///     A SCIM identity GitLab associated with a user through group provisioning, as embedded in
///     <see cref="GitLabUser.ScimIdentities" />.
/// </summary>
/// <remarks>
///     The OpenAPI document inconsistently calls <see cref="GroupId" /> and <see cref="Active" />
///     strings, while GitLab's response payload uses the identifier and Boolean values represented here.
///     The source-generated JSON context accepts a quoted numeric group ID as well, preserving
///     compatibility with older self-managed instances.
/// </remarks>
public sealed record GitLabScimIdentity
{
    /// <summary>The identifier supplied by the external SCIM provider.</summary>
    public string? ExternUid { get; init; }

    /// <summary>The group whose SCIM integration provisioned the identity.</summary>
    public long? GroupId { get; init; }

    /// <summary>Whether the identity is currently active at the SCIM provider.</summary>
    public bool? Active { get; init; }
}