namespace GitLab.Client.Models.Requests;

/// <summary>
///     Settings for the Diffblue Cover integration
///     (<c>PUT /projects/:id/integrations/diffblue-cover</c>) - automated Java unit-test generation.
/// </summary>
public sealed record DiffblueCoverIntegrationRequest
{
    /// <summary>Diffblue Cover license key.</summary>
    public required string DiffblueLicenseKey { get; init; }

    /// <summary>Access token name used by Diffblue Cover in pipelines.</summary>
    public required string DiffblueAccessTokenName { get; init; }

    /// <summary>Access token secret used by Diffblue Cover in pipelines.</summary>
    public required string DiffblueAccessTokenSecret { get; init; }

    /// <summary>Indicates whether to inherit the default settings. Defaults to <c>false</c>.</summary>
    public bool? UseInheritedSettings { get; init; }
}