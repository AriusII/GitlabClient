using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The minimum role required to push or delete a matching container image or tag, on the
///     <em>update</em> request for a <see cref="GitLabContainerRegistryProtectionRule" /> or a
///     <see cref="GitLabContainerRegistryProtectionTagRule" />.
///     <para>
///         Unlike the create request's <see cref="GitLabContainerRegistryProtectionAccessLevel" />, GitLab
///         accepts an explicit empty string here to clear the restriction entirely - which is exactly
///         what <see cref="Unset" /> serializes to. Leaving the property <c>null</c> instead omits it from
///         the request and keeps the rule's current value; there is no C# value that can express "send an
///         explicit empty string" other than a dedicated enum member.
///     </para>
///     <para>
///         This uses a hand-written <see cref="JsonConverter{T}" /> rather than the usual generic
///         <c>JsonStringEnumConverter&lt;T&gt;</c>: the source-generated enum converter validates every
///         member's wire name at start-up and unconditionally rejects an empty string, so
///         <see cref="Unset" /> could never be registered through it.
///     </para>
/// </summary>
[JsonConverter(typeof(GitLabContainerRegistryProtectionAccessLevelOrUnsetConverter))]
public enum GitLabContainerRegistryProtectionAccessLevelOrUnset
{
    Maintainer,
    Owner,
    Admin,

    /// <summary>Clears the restriction. Serializes to an explicit empty string, not <c>null</c>.</summary>
    Unset
}