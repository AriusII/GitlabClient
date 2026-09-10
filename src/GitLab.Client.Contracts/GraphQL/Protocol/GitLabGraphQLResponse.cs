using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitLab.Client.GraphQL.Protocol;

/// <summary>
///     The GraphQL response envelope returned by GitLab for one operation.
/// </summary>
/// <typeparam name="TData">The operation-specific shape selected by the GraphQL document.</typeparam>
/// <remarks>
///     GraphQL permits partial data and top-level errors in the same response. Consequently,
///     <see cref="Data" /> and <see cref="Errors" /> are intentionally independent nullable members.
/// </remarks>
public sealed record GitLabGraphQLResponse<TData>
{
    /// <summary>The operation data, which can be partial when <see cref="Errors" /> is also populated.</summary>
    [JsonPropertyName("data")]
    public TData? Data { get; init; }

    /// <summary>The top-level GraphQL errors, when GitLab reports one or more execution errors.</summary>
    [JsonPropertyName("errors")]
    public IReadOnlyList<GitLabGraphQLError>? Errors { get; init; }

    /// <summary>
    ///     Operation- or server-defined extension data. GitLab does not offer one stable schema for this member,
    ///     so its complete JSON value is preserved.
    /// </summary>
    [JsonPropertyName("extensions")]
    public JsonElement? Extensions { get; init; }

    /// <summary>Gets whether GitLab returned at least one top-level GraphQL error.</summary>
    [JsonIgnore]
    public bool HasErrors => Errors is { Count: > 0 };
}