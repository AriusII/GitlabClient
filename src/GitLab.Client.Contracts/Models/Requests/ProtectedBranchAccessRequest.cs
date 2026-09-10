using System.Text.Json.Serialization;

namespace GitLab.Client.Models.Requests;

/// <summary>
///     One entry of the <c>allowed_to_push</c>, <c>allowed_to_merge</c> or <c>allowed_to_unprotect</c>
///     arrays on <see cref="ProtectBranchRequest" /> and <see cref="UpdateProtectedBranchRequest" />.
///     Exactly one of <see cref="UserId" />, <see cref="GroupId" />, <see cref="DeployKeyId" />,
///     <see cref="MemberRoleId" /> or <see cref="AccessLevel" /> is meant to be set per entry.
///     <para>
///         <see cref="Id" /> and <see cref="Destroy" /> exist only on the update form of this item - they
///         edit or delete an entry that already exists. They are omitted from a create payload by the
///         library's <c>WhenWritingNull</c> policy, so one type serves both verbs.
///     </para>
/// </summary>
public sealed record ProtectedBranchAccessRequest
{
    /// <summary>The ID of a user allowed to perform the action.</summary>
    public long? UserId { get; init; }

    /// <summary>The ID of a group allowed to perform the action.</summary>
    public long? GroupId { get; init; }

    /// <summary>The ID of a deploy key allowed to push. Push rules only.</summary>
    public long? DeployKeyId { get; init; }

    /// <summary>The role allowed to perform the action - 30 Developer, 40 Maintainer, 60 Admin, 0 no one.</summary>
    public int? AccessLevel { get; init; }

    /// <summary>
    ///     The Ultimate custom member role allowed to perform the action. GitLab introduced this in 19.2
    ///     behind the <c>custom_roles_for_protected_branches</c> feature flag.
    /// </summary>
    public long? MemberRoleId { get; init; }

    /// <summary>The ID of the existing entry to edit. Update only.</summary>
    public long? Id { get; init; }

    /// <summary>
    ///     Set to <c>true</c>, together with <see cref="Id" />, to delete the entry. Update only. The wire
    ///     name is spelled out because the snake_case policy would emit <c>destroy</c> and the entry would
    ///     silently survive.
    /// </summary>
    [JsonPropertyName("_destroy")]
    public bool? Destroy { get; init; }
}