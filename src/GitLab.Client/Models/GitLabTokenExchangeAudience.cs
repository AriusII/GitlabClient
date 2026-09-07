using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The modular-service audience a <c>POST /token_exchange</c> JWT is scoped to.
///     <para>
///         GitLab's spec enumerates exactly one member today (<c>gitlab-artifact-registry</c>). Token
///         exchange is young, explicitly experimental surface (<c>x-gitlab-lifecycle: experiment</c> in
///         the spec) - expect this vocabulary to grow, and expect it to grow with a GitLab release rather
///         than a query to this library.
///     </para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabTokenExchangeAudience>))]
public enum GitLabTokenExchangeAudience
{
    /// <summary>The GitLab-hosted Artifact Registry modular service.</summary>
    [JsonStringEnumMemberName("gitlab-artifact-registry")]
    GitlabArtifactRegistry
}