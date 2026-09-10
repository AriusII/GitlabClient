namespace GitLab.Client.Models.Requests;

/// <summary>
///     Initial assets nested in <see cref="CreateReleaseRequest.Assets" />. GitLab creates every supplied
///     link atomically with the release.
/// </summary>
public sealed record CreateReleaseAssetsRequest
{
    /// <summary>The asset links to create with the release.</summary>
    public IReadOnlyList<CreateReleaseLinkRequest>? Links { get; init; }
}