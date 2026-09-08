namespace GitLab.Client.Models;

/// <summary>
///     Typed settings for the <c>google-play</c> integration - see
///     <see cref="GitLabIntegrationSlug.GooglePlay" />. Uploads build artifacts from CI/CD to the
///     Google Play console using a service account key.
/// </summary>
public sealed record GooglePlayIntegrationRequest
{
    /// <summary>Package name of the app in Google Play.</summary>
    public required string PackageName { get; init; }

    /// <summary>File name of the Google Play service account key.</summary>
    public required string ServiceAccountKeyFileName { get; init; }

    /// <summary>
    ///     The Google Play service account key - the raw contents of the JSON key file, not a path to
    ///     one.
    /// </summary>
    public required string ServiceAccountKey { get; init; }

    /// <summary>Set variables on protected branches and tags only.</summary>
    public bool? GooglePlayProtectedRefs { get; init; }

    /// <summary>Indicates whether to inherit the default settings. Defaults to <see langword="false" />.</summary>
    public bool? UseInheritedSettings { get; init; }
}