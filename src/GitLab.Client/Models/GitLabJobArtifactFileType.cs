using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Which of a job's artifacts to download from
///     <c>GET /projects/:id/jobs/:job_id/artifacts</c>. Omitting it downloads the job's artifacts archive.
///     <para>
///         This is an outbound-only vocabulary - it is a query parameter that GitLab validates and rejects
///         with a 400, never something GitLab sends back - so an enum here can only ever prevent a bad
///         request, not fail to read a response. That is why it is an enum while
///         <see cref="GitLabJobArtifact.FileType" />, which is the same vocabulary read from GitLab, stays a
///         string.
///     </para>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabJobArtifactFileType>))]
public enum GitLabJobArtifactFileType
{
    [JsonStringEnumMemberName("accessibility")]
    Accessibility,

    [JsonStringEnumMemberName("api_fuzzing")]
    ApiFuzzing,

    /// <summary>The job's artifacts archive - the same thing GitLab returns when no type is given.</summary>
    [JsonStringEnumMemberName("archive")] Archive,

    [JsonStringEnumMemberName("cobertura")]
    Cobertura,

    [JsonStringEnumMemberName("jacoco")] Jacoco,

    [JsonStringEnumMemberName("codequality")]
    CodeQuality,

    [JsonStringEnumMemberName("container_scanning")]
    ContainerScanning,

    [JsonStringEnumMemberName("dast")] Dast,

    [JsonStringEnumMemberName("dependency_scanning")]
    DependencyScanning,

    [JsonStringEnumMemberName("dotenv")] Dotenv,

    [JsonStringEnumMemberName("junit")] JUnit,

    [JsonStringEnumMemberName("license_scanning")]
    LicenseScanning
}