using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The file formats <c>POST /projects/:id/dependency_list_exports</c> can produce.
/// </summary>
/// <remarks>
///     The three export levels accept different vocabularies - a group takes
///     <see cref="GitLabGroupDependencyListExportType" /> and a pipeline takes
///     <see cref="GitLabPipelineDependencyListExportType" /> - so each level has its own enum rather than
///     one union that would let an invalid combination compile.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabProjectDependencyListExportType>))]
public enum GitLabProjectDependencyListExportType
{
    /// <summary>GitLab's own dependency list JSON. The default when the request omits the format.</summary>
    [JsonStringEnumMemberName("dependency_list")]
    DependencyList,

    /// <summary>Comma-separated values.</summary>
    [JsonStringEnumMemberName("csv")] Csv,

    /// <summary>CycloneDX 1.6, as JSON.</summary>
    [JsonStringEnumMemberName("cyclonedx_1_6_json")]
    CycloneDx16Json
}