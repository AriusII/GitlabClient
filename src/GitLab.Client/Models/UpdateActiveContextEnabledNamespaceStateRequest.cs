using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     Body for <c>PUT /admin/active_context/code/enabled_namespaces</c>, which transitions the ActiveContext
///     code indexing state of one namespace.
/// </summary>
public sealed record UpdateActiveContextEnabledNamespaceStateRequest
{
    /// <summary>
    ///     The namespace's numeric ID or its URL-encoded full path. The spec types this field as either a
    ///     JSON string or a JSON number, so it is carried as a raw <see cref="JsonElement" /> rather than
    ///     forced into one C# type - build it with, for example,
    ///     <c>JsonDocument.Parse("123").RootElement</c> for a numeric ID or
    ///     <c>JsonDocument.Parse("\"group/subgroup\"").RootElement</c> for a path.
    /// </summary>
    public required JsonElement NamespaceId { get; init; }

    public required GitLabActiveContextNamespaceState State { get; init; }

    /// <summary>Defaults to the active connection when omitted.</summary>
    public long? ConnectionId { get; init; }
}