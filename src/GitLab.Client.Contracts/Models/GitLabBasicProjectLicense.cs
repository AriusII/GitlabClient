using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Models;

/// <summary>The <c>APIEntitiesLicenseBasic</c> projection embedded by a basic project.</summary>
public sealed record GitLabBasicProjectLicense
{
    public string? Key { get; init; }

    public string? Name { get; init; }

    public string? Nickname { get; init; }

    /// <summary>The license HTML location as the unconstrained string returned by GitLab.</summary>
    [SuppressMessage("Design", "CA1056",
        Justification =
            "APIEntitiesLicenseBasic declares html_url as string without a URI format; the contract does "
            + "not guarantee that it is absolute or parseable by System.Uri.")]
    public string? HtmlUrl { get; init; }

    /// <summary>The license source location as the unconstrained string returned by GitLab.</summary>
    [SuppressMessage("Design", "CA1056",
        Justification =
            "APIEntitiesLicenseBasic declares source_url as string without a URI format; preserving the "
            + "wire value is safer than imposing an undocumented Uri invariant.")]
    public string? SourceUrl { get; init; }
}