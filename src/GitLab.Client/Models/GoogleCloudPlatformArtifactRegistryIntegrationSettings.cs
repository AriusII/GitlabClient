namespace GitLab.Client.Models;

/// <summary>
///     Typed settings for the <c>google-cloud-platform-artifact-registry</c> integration - see
///     <see cref="GitLabIntegrationSlug.GoogleCloudPlatformArtifactRegistry" />. Pass this to
///     <c>IIntegrationsClient.SetAsync{TSettings}</c> together with its
///     <see cref="Infrastructure.Serialization.GitLabJsonContext" /> entry.
///     <para>
///         All three identifying fields are spec-required even when
///         <see cref="UseInheritedSettings" /> is set, so this library does not make them optional on
///         the caller's behalf - GitLab, not this client, is the authority on whether inheriting still
///         needs them populated.
///     </para>
/// </summary>
public sealed record GoogleCloudPlatformArtifactRegistryIntegrationSettings
{
    /// <summary>ID of the Google Cloud project.</summary>
    public required string ArtifactRegistryProjectId { get; init; }

    /// <summary>Repository of Artifact Registry.</summary>
    public required string ArtifactRegistryRepositories { get; init; }

    /// <summary>Location of the Artifact Registry repository.</summary>
    public required string ArtifactRegistryLocation { get; init; }

    /// <summary>Indicates whether to inherit the default settings. Defaults to <see langword="false" />.</summary>
    public bool? UseInheritedSettings { get; init; }
}