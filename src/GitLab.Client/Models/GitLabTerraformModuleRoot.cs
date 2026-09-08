using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     The root module's dependency and provider information, nested within a
///     <see cref="GitLabTerraformModule" /> or a <see cref="GitLabTerraformModuleVersionInfo" />.
/// </summary>
public sealed record GitLabTerraformModuleRoot
{
    /// <summary>
    ///     The root module's dependencies. GitLab has not published a concrete schema for a populated entry
    ///     - every documented example shows an empty array - so each entry is left as the raw JSON element
    ///     rather than risk silently dropping fields under a guessed shape.
    /// </summary>
    public IReadOnlyList<JsonElement>? Dependencies { get; init; }

    /// <summary>The provider versions the root module requires.</summary>
    public IReadOnlyList<GitLabTerraformModuleProviderVersion>? Providers { get; init; }
}