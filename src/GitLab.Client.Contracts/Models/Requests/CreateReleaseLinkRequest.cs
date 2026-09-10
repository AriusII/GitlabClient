namespace GitLab.Client.Models.Requests;

/// <summary>Request body for <c>POST /projects/:id/releases/:tag_name/assets/links</c>.</summary>
public sealed record CreateReleaseLinkRequest
{
    /// <summary>The name of the link. Must be unique within the release.</summary>
    public required string Name { get; init; }

    /// <summary>The URL the link points at. Must be unique within the release.</summary>
    public required Uri Url { get; init; }

    /// <summary>
    ///     Optional path for a permanent GitLab-hosted URL that redirects to <see cref="Url" />, for example
    ///     <c>/binaries/linux-amd64</c>. Supersedes the deprecated <c>filepath</c> parameter, which this
    ///     library deliberately does not expose.
    /// </summary>
    public string? DirectAssetPath { get; init; }

    /// <summary>Defaults to <see cref="GitLabReleaseLinkType.Other" /> when omitted.</summary>
    public GitLabReleaseLinkType? LinkType { get; init; }
}