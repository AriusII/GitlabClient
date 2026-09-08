using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The publication status of a package in the cross-format registry summary
///     (<see cref="GitLabPackage.Status" />, and the <c>status</c> filter on <see cref="PackageListOptions" />
///     and <see cref="GroupPackageListOptions" />). Distinct from <see cref="GitLabPackageFileStatus" />,
///     which is a package FILE's narrower two-value publication state.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabPackageStatus>))]
public enum GitLabPackageStatus
{
    [JsonStringEnumMemberName("default")] Default,

    [JsonStringEnumMemberName("hidden")] Hidden,

    [JsonStringEnumMemberName("processing")]
    Processing,

    [JsonStringEnumMemberName("error")] Error,

    [JsonStringEnumMemberName("pending_destruction")]
    PendingDestruction,

    [JsonStringEnumMemberName("deprecated")]
    Deprecated
}