using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     Which kind of artifact a runner is uploading via <c>POST /jobs/:id/artifacts</c> or declaring in
///     <c>POST /jobs/:id/artifacts/authorize</c> - the spec pins this to a closed vocabulary, and it is
///     an outbound-only value (a request field GitLab validates, never something it sends back), so an
///     enum here can only ever prevent a bad request.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabJobArtifactUploadType>))]
public enum GitLabJobArtifactUploadType
{
    /// <summary>The job's whole artifacts archive - the default when nothing else applies.</summary>
    [JsonStringEnumMemberName("archive")] Archive,

    [JsonStringEnumMemberName("metadata")] Metadata,

    /// <summary>The job's own log, uploaded as an artifact rather than appended via the trace endpoint.</summary>
    [JsonStringEnumMemberName("trace")] Trace,

    [JsonStringEnumMemberName("junit")] JUnit,

    [JsonStringEnumMemberName("sast")] Sast,

    [JsonStringEnumMemberName("dependency_scanning")]
    DependencyScanning,

    [JsonStringEnumMemberName("container_scanning")]
    ContainerScanning,

    [JsonStringEnumMemberName("dast")] Dast,

    [JsonStringEnumMemberName("codequality")]
    CodeQuality,

    [JsonStringEnumMemberName("license_scanning")]
    LicenseScanning,

    [JsonStringEnumMemberName("performance")]
    Performance,

    [JsonStringEnumMemberName("metrics")] Metrics
}