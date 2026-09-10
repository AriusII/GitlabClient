using GitLab.Client.Serialization;

namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body for <c>PATCH /admin/zoekt/namespaces/:id</c>, which overrides how many replicas an enabled
///     namespace is indexed with.
/// </summary>
public sealed record UpdateZoektNamespaceReplicasRequest
{
    /// <summary>
    ///     The replica-count override to set. GitLab's spec documents <c>null</c> as clearing the override,
    ///     but this client's serializer omits null properties from the request body entirely (see
    ///     <see cref="GitLabJsonContext" />), so there is currently no way to
    ///     send an explicit clear through this method - only to set a positive override.
    /// </summary>
    public int? NumberOfReplicasOverride { get; init; }
}