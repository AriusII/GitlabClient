namespace GitLab.Client.Models;

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
    /// <summary>The value to store. Sent verbatim; GitLab keeps it as an opaque string.</summary>
    public required string Value { get; init; }
}