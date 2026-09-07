using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     The npm registry metadata document for one package - what an <c>npm install</c> or
///     <c>npm view</c> run against a GitLab-backed npm registry reads back.
///     <para>
///         GitLab's spec types <c>versions</c> and <c>dist-tags</c> as a bare <c>object</c> with no
///         further schema. <see cref="Versions" /> is keyed by version string, and each value is a full
///         npm manifest for that version (name, version, <c>dist.shasum</c>, <c>dist.tarball</c>, ...)
///         whose shape npm itself controls, so it stays a raw <see cref="JsonElement" /> rather than an
///         invented DTO. <see cref="DistTags" /> is simpler - tag name to version string - and is typed
///         accordingly.
///     </para>
/// </summary>
public sealed record GitLabNpmPackage
{
    /// <summary>The package name, including its <c>@scope/</c> prefix for a scoped package.</summary>
    public string? Name { get; init; }

    /// <summary>Every published version of the package, keyed by version string.</summary>
    public IReadOnlyDictionary<string, JsonElement>? Versions { get; init; }

    /// <summary>The package's dist-tags (for example <c>latest</c>), keyed by tag name to version string.</summary>
    [JsonPropertyName("dist-tags")]
    public IReadOnlyDictionary<string, string>? DistTags { get; init; }
}