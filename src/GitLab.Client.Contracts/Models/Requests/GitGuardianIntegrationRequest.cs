namespace GitLab.Client.Models.Requests;

/// <summary>
///     Settings for the GitGuardian integration (<c>PUT /projects/:id/integrations/git-guardian</c>) -
///     blocks pushes that contain leaked secrets.
/// </summary>
public sealed record GitGuardianIntegrationRequest
{
    /// <summary>
    ///     GitGuardian API base URL. Defaults to <c>https://api.gitguardian.com</c>. Use
    ///     <c>https://api.eu1.gitguardian.com</c> for the EU region, or the URL of your self-managed
    ///     GitGuardian instance.
    /// </summary>
    public Uri? ApiUrl { get; init; }

    /// <summary>Personal access token to authenticate calls to the GitGuardian API.</summary>
    public required string Token { get; init; }

    /// <summary>Indicates whether to inherit the default settings. Defaults to <c>false</c>.</summary>
    public bool? UseInheritedSettings { get; init; }
}