using System.Text.Json.Serialization;

namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body of <c>PUT /users/:id/custom_attributes/:key</c>, <c>PUT /groups/:id/custom_attributes/:key</c>
///     and <c>PUT /projects/:id/custom_attributes/:key</c>. The key travels in the route, so only the value
///     is sent.
/// </summary>
/// <remarks>
///     The endpoint is an upsert: GitLab creates the attribute when the key is new and overwrites it when
///     it is not, answering <c>200</c> either way rather than distinguishing <c>201</c>.
/// </remarks>
public sealed record SetCustomAttributeRequest
{
    /// <summary>
    ///     The value to store. The field itself is required by GitLab, but its value may be <see langword="null" />
    ///     according to the 19.4 OpenAPI schema. It therefore opts out of the context's normal null omission:
    ///     <c>{ "value": null }</c> is a distinct valid request from an omitted required field.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required string? Value { get; init; }
}