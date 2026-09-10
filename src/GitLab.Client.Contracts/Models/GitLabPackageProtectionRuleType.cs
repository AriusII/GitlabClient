using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The package format a <see cref="GitLabPackageProtectionRule" /> applies to
///     (<c>package_type</c> on the create/update request).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabPackageProtectionRuleType>))]
public enum GitLabPackageProtectionRuleType
{
    [JsonStringEnumMemberName("cargo")] Cargo,

    [JsonStringEnumMemberName("conan")] Conan,

    [JsonStringEnumMemberName("generic")] Generic,

    [JsonStringEnumMemberName("helm")] Helm,

    [JsonStringEnumMemberName("maven")] Maven,

    [JsonStringEnumMemberName("npm")] Npm,

    [JsonStringEnumMemberName("nuget")] NuGet,

    [JsonStringEnumMemberName("pypi")] PyPi,

    [JsonStringEnumMemberName("terraform_module")]
    TerraformModule
}