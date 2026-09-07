using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     One entry of the <c>approval_rules</c> array on <see cref="ProtectEnvironmentRequest" /> and
///     <see cref="UpdateProtectedEnvironmentRequest" />. Exactly one of <see cref="UserId" />,
///     <see cref="GroupId" /> or <see cref="AccessLevel" /> is meant to be set per entry, with
///     <see cref="RequiredApprovals" /> saying how many approvals it contributes.
///     <para>
///         As with <see cref="DeployAccessLevelRequest" />, <see cref="Id" /> and <see cref="Destroy" />
///         belong to the update form only and are omitted from a create payload.
///     </para>
/// </summary>
public sealed record ProtectedEnvironmentApprovalRuleRequest
{
    /// <summary>The ID of a user who may approve.</summary>
    public long? UserId { get; init; }

    /// <summary>The ID of a group whose members may approve.</summary>
    public long? GroupId { get; init; }

    /// <summary>0 for direct group membership, 1 to include all inherited groups. Defaults to 0.</summary>
    public int? GroupInheritanceType { get; init; }

    /// <summary>The role that may approve - 20 Reporter, 30 Developer, 40 Maintainer, 60 Admin.</summary>
    public int? AccessLevel { get; init; }

    /// <summary>How many approvals this rule requires.</summary>
    public int? RequiredApprovals { get; init; }

    /// <summary>The ID of the existing rule to edit. Update only.</summary>
    public long? Id { get; init; }

    /// <summary>Set to <c>true</c>, together with <see cref="Id" />, to delete the rule. Update only.</summary>
    [JsonPropertyName("_destroy")]
    public bool? Destroy { get; init; }
}