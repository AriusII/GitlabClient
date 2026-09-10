using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     A package format in the cross-format package registry summary - <see cref="GitLabPackage.PackageType" />,
///     and the <c>package_type</c> filter on <see cref="PackageListOptions" /> and
///     <see cref="GroupPackageListOptions" />. Distinct from <see cref="GitLabPackageProtectionRuleType" />,
///     which enumerates a narrower set of formats package protection rules support.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabPackageType>))]
public enum GitLabPackageType
{
    [JsonStringEnumMemberName("maven")] Maven,

    [JsonStringEnumMemberName("npm")] Npm,

    [JsonStringEnumMemberName("conan")] Conan,

    [JsonStringEnumMemberName("nuget")] NuGet,

    [JsonStringEnumMemberName("pypi")] PyPi,

    [JsonStringEnumMemberName("composer")] Composer,

    [JsonStringEnumMemberName("generic")] Generic,

    [JsonStringEnumMemberName("golang")] Golang,

    [JsonStringEnumMemberName("debian")] Debian,

    [JsonStringEnumMemberName("rubygems")] RubyGems,

    [JsonStringEnumMemberName("helm")] Helm,

    [JsonStringEnumMemberName("terraform_module")]
    TerraformModule,

    /// <summary>The RPM package registry format.</summary>
    [JsonStringEnumMemberName("rpm")] Rpm,

    /// <summary>The ML Model Registry package format.</summary>
    [JsonStringEnumMemberName("ml_model")] MlModel,

    /// <summary>The Cargo crate registry format.</summary>
    [JsonStringEnumMemberName("cargo")] Cargo
}