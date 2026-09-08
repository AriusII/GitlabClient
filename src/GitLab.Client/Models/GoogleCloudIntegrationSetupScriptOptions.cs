using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>
///     Filters for <c>GET /projects/:id/google_cloud/setup/integrations.sh</c>. Experimental GitLab
///     surface (<c>x-gitlab-lifecycle: experiment</c> in the spec).
/// </summary>
[GitLabQuery]
public readonly record struct GoogleCloudIntegrationSetupScriptOptions
{
    /// <summary>Whether the generated script should also enable the Google Artifact Management integration.</summary>
    public bool? EnableGoogleCloudArtifactRegistry { get; init; }

    /// <summary>
    ///     The Google Cloud project ID the Artifact Registry integration should point at. Required when
    ///     <see cref="EnableGoogleCloudArtifactRegistry" /> is <see langword="true" />.
    /// </summary>
    public string? GoogleCloudArtifactRegistryProjectId { get; init; }
}