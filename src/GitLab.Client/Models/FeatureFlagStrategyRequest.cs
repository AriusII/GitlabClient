using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     One entry of the <c>strategies</c> array on <see cref="CreateFeatureFlagRequest" /> and
///     <see cref="UpdateFeatureFlagRequest" />.
///     <para>
///         <see cref="Id" /> and <see cref="Destroy" /> exist only on the update form - they edit or
///         delete a strategy that already exists. They are omitted from a create payload by the library's
///         <c>WhenWritingNull</c> policy, so one type serves both verbs.
///     </para>
/// </summary>
public sealed record FeatureFlagStrategyRequest
{
    /// <summary>
    ///     The Unleash strategy name, in Unleash's own camelCase: <c>default</c>,
    ///     <c>gradualRolloutUserId</c>, <c>userWithId</c>, <c>flexibleRollout</c> or <c>gitlabUserList</c>.
    ///     GitLab's usual snake_case is not accepted here.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    ///     The strategy's parameters, whose members differ per <see cref="Name" />. The spec types each
    ///     strategy as an untyped object, so this stays a raw <see cref="JsonElement" /> rather than a shape
    ///     GitLab does not promise: <c>percentage</c> plus <c>groupId</c> for <c>flexibleRollout</c>,
    ///     <c>userIds</c> for <c>userWithId</c>, an empty object for <c>default</c>.
    /// </summary>
    public JsonElement? Parameters { get; init; }

    /// <summary>The environments the strategy applies to. GitLab scopes it to every environment when unset.</summary>
    public IReadOnlyList<FeatureFlagStrategyScopeRequest>? Scopes { get; init; }

    /// <summary>
    ///     The <see cref="GitLabFeatureFlagUserList.Id" /> of the list to target. Only the
    ///     <c>gitlabUserList</c> strategy reads it.
    /// </summary>
    public long? UserListId { get; init; }

    /// <summary>The ID of the existing strategy to edit. Update only.</summary>
    public long? Id { get; init; }

    /// <summary>
    ///     Set to <c>true</c>, together with <see cref="Id" />, to delete the strategy. Update only. The wire
    ///     name is spelled out because the snake_case policy would emit <c>destroy</c> and the entry would
    ///     silently survive.
    /// </summary>
    [JsonPropertyName("_destroy")]
    public bool? Destroy { get; init; }
}