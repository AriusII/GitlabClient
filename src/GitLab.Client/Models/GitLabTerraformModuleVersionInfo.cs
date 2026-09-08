using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>One published version within a <see cref="GitLabTerraformModuleVersionsEntry" />.</summary>
public sealed record GitLabTerraformModuleVersionInfo
{
    /// <summary>The version identifier, for example <c>1.0.0</c>.</summary>
    public string? Version { get; init; }

    /// <summary>
    ///     Nested submodules, in the shape the Terraform Module Registry Protocol defines. GitLab has not
    ///     published a concrete schema for a populated entry - every documented example shows an empty
    ///     array - so each entry is left as the raw JSON element rather than risk silently dropping fields
    ///     under a guessed shape.
    /// </summary>
    public IReadOnlyList<JsonElement>? Submodules { get; init; }

    /// <summary>The root module's dependency and provider information for this version.</summary>
    public GitLabTerraformModuleRoot? Root { get; init; }
}