using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The file formats <c>POST /pipelines/:id/dependency_list_exports</c> can produce. GitLab declares
///     exactly one, which is also the default.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabPipelineDependencyListExportType>))]
public enum GitLabPipelineDependencyListExportType
{
    /// <summary>A merged CycloneDX SBOM for the whole pipeline.</summary>
    [JsonStringEnumMemberName("sbom")] Sbom
}