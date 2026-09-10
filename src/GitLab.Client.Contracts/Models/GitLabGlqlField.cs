using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     One column of a <c>POST /glql</c> result: how to address a value inside
///     <see cref="GitLabGlqlData.Nodes" />, and how to label it.
/// </summary>
public sealed record GitLabGlqlField
{
    /// <summary>The key this field appears under in each node - for a parameterised field, its alias (<c>p50</c>).</summary>
    public string? Key { get; init; }

    /// <summary>Human-readable column label (<c>Title</c>).</summary>
    public string? Label { get; init; }

    /// <summary>
    ///     The underlying field name, usually the same as <see cref="Key" /> but different where one field
    ///     has several spellings (<c>created</c> and <c>createdAt</c>).
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    ///     The base field behind an alias - <c>durationQuantile</c> where <see cref="Key" /> is <c>p50</c>.
    ///     Equal to <see cref="Key" /> for a standard field.
    /// </summary>
    public string? Field { get; init; }

    /// <summary>
    ///     Field classification, <c>dimension</c> or <c>metric</c>, for analytics-mode queries; absent for
    ///     standard fields. A bare string rather than an enum: the spec documents the two values in prose
    ///     but does not enumerate them, so an unknown third value must not fail the whole response.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    ///     Resolved parameter metadata for a parameterised field (for example <c>granularity: weekly</c>),
    ///     absent when the field takes none. Untyped in the spec, so it is surfaced as a raw
    ///     <see cref="JsonElement" />.
    /// </summary>
    public JsonElement? Parameters { get; init; }
}