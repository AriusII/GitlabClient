namespace GitLab.Client.Models.Requests;

/// <summary>
///     Settings for the External Wiki integration
///     (<c>PUT /projects/:id/integrations/external-wiki</c>) - replaces the project wiki link with an
///     external URL.
/// </summary>
public sealed record ExternalWikiIntegrationRequest
{
    /// <summary>URL of the external wiki.</summary>
    public required Uri ExternalWikiUrl { get; init; }

    /// <summary>Indicates whether to inherit the default settings. Defaults to <c>false</c>.</summary>
    public bool? UseInheritedSettings { get; init; }
}