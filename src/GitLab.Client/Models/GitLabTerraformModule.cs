using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     A Terraform module's registry metadata, as returned by the Terraform Module Registry API's
///     "retrieve latest version" and "retrieve specific version" endpoints
///     (<c>/packages/terraform/modules/v1/:module_namespace/:module_name/:module_system[/:module_version]</c>).
///     GitLab's response is a deliberately pruned subset of the community Terraform Module Registry
///     Protocol schema - most detail the full protocol allows (readme, inputs, outputs, resources) is not
///     present in GitLab's implementation.
/// </summary>
public sealed record GitLabTerraformModule
{
    /// <summary>The module's fully qualified name, formatted as <c>module_name/module_system</c>.</summary>
    public string? Name { get; init; }

    /// <summary>The module system (provider), for example <c>local</c> or <c>aws</c>.</summary>
    public string? Provider { get; init; }

    /// <summary>The providers this module declares.</summary>
    public IReadOnlyList<string>? Providers { get; init; }

    /// <summary>The version this response describes - the latest published version when queried without one.</summary>
    public string? Version { get; init; }

    /// <summary>Every published version of this module.</summary>
    public IReadOnlyList<string>? Versions { get; init; }

    /// <summary>The module's source repository.</summary>
    public Uri? Source { get; init; }

    /// <summary>The root module's dependency and provider information.</summary>
    public GitLabTerraformModuleRoot? Root { get; init; }

    /// <summary>
    ///     Nested submodules, in the shape the Terraform Module Registry Protocol defines. GitLab has not
    ///     published a concrete schema for a populated entry - every documented example shows an empty
    ///     array - so each entry is left as the raw JSON element rather than risk silently dropping fields
    ///     under a guessed shape.
    /// </summary>
    public IReadOnlyList<JsonElement>? Submodules { get; init; }
}