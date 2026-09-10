namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PUT /projects/:id/releases/:tag_name/assets/links/:link_id</c>. Every property
///     is optional: the library's <c>WhenWritingNull</c> policy omits the ones left unset, so an update
///     touches only the fields it names.
/// </summary>
public sealed record UpdateReleaseLinkRequest
{
    public string? Name { get; init; }

    public Uri? Url { get; init; }

    /// <summary>See <see cref="CreateReleaseLinkRequest.DirectAssetPath" />.</summary>
    public string? DirectAssetPath { get; init; }

    public GitLabReleaseLinkType? LinkType { get; init; }
}