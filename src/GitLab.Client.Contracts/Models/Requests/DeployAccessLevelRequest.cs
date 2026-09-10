using System.Text.Json.Serialization;

namespace GitLab.Client.Models.Requests;

/// <summary>
///     One entry of the <c>deploy_access_levels</c> array on
///     <see cref="ProtectEnvironmentRequest" /> and <see cref="UpdateProtectedEnvironmentRequest" />.
///     Exactly one of <see cref="UserId" />, <see cref="GroupId" /> or <see cref="AccessLevel" /> is
///     meant to be set per entry.
///     <para>
///         <see cref="Id" /> and <see cref="Destroy" /> exist only on the update form of this item - they
///         edit or delete an entry that already exists. They are omitted from a create payload by the
///         library's <c>WhenWritingNull</c> policy, so one type serves both verbs.
///     </para>
/// </summary>
public sealed record DeployAccessLevelRequest
{
    /// <summary>The ID of a user allowed to deploy.</summary>
    public long? UserId { get; init; }

    /// <summary>The ID of a group allowed to deploy.</summary>
    public long? GroupId { get; init; }

    /// <summary>0 for direct group membership, 1 to include all inherited groups. Defaults to 0.</summary>
    public int? GroupInheritanceType { get; init; }

    /// <summary>The role allowed to deploy - 20 Reporter, 30 Developer, 40 Maintainer, 60 Admin.</summary>
    public int? AccessLevel { get; init; }

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