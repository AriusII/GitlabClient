using GitLab.Client.SourceGenerators;

namespace GitLab.Client.Models;

/// <summary>Filters and pagination for <c>GET /admin/data_management/:model_name</c>.</summary>
[GitLabQuery]
public sealed record AdminModelListOptions
{
    public int? Page { get; init; }

    public int? PerPage { get; init; }

    /// <summary>
    ///     Restricts the list to these record identifiers. GitLab accepts either numbers or strings here
    ///     depending on the model, so every element is sent as text; Grape coerces it back to the
    ///     underlying column type.
    /// </summary>
    public IReadOnlyList<string>? Identifiers { get; init; }

    /// <summary>
    ///     <c>pending</c>, <c>started</c>, <c>succeeded</c>, <c>failed</c> or <c>disabled</c>. Left as free
    ///     text rather than an enum: the write-side <c>checksum_state</c> on
    ///     <see cref="RecalculateModelChecksumsRequest" /> declares a narrower vocabulary for the same
    ///     wire name, so one shared enum would either reject a valid filter value or accept an invalid
    ///     write value.
    /// </summary>
    public string? ChecksumState { get; init; }

    /// <summary>Opaque cursor from a previous page, for cursor-based pagination as an alternative to <see cref="Page" />.</summary>
    public string? Cursor { get; init; }

    public AdminModelSortDirection? Sort { get; init; }
}