using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     One Unleash strategy attached to a <see cref="GitLabFeatureFlag" /> - the rule that decides who
///     the flag is on for, and in which environments.
/// </summary>
public sealed record GitLabFeatureFlagStrategy
{
    public long? Id { get; init; }

    /// <summary>
    ///     The Unleash strategy name. GitLab passes Unleash's own camelCase vocabulary straight through, so
    ///     the values are <c>default</c>, <c>gradualRolloutUserId</c>, <c>userWithId</c>,
    ///     <c>flexibleRollout</c> and <c>gitlabUserList</c> - not GitLab's usual snake_case. The spec types it
    ///     as a bare string with no enumeration, so it is not projected onto an enum: a strategy Unleash adds
    ///     later must not turn a healthy response into a deserialization failure.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    ///     The strategy's parameters, whose members differ per <see cref="Name" /> (<c>percentage</c> and
    ///     <c>groupId</c> for <c>flexibleRollout</c>, <c>userIds</c> for <c>userWithId</c>, nothing for
    ///     <c>default</c>). Surfaced as a raw <see cref="JsonElement" /> rather than forced into a shape the
    ///     spec does not promise.
    /// </summary>
    public JsonElement? Parameters { get; init; }

    /// <summary>The environments this strategy applies to.</summary>
    public IReadOnlyList<GitLabFeatureFlagScope>? Scopes { get; init; }

    /// <summary>The user list this strategy targets, populated only for the <c>gitlabUserList</c> strategy.</summary>
    public GitLabFeatureFlagBasicUserList? UserList { get; init; }
}