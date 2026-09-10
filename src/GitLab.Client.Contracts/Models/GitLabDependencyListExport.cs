namespace GitLab.Client.Models;

/// <summary>
///     A dependency list export job (<c>/projects/:id/dependency_list_exports</c>,
///     <c>/groups/:id/dependency_list_exports</c>, <c>/pipelines/:id/dependency_list_exports</c>) - the
///     handle returned when an export is requested, and polled through
///     <c>GET /dependency_list_exports/:export_id</c> until <see cref="HasFinished" /> is
///     <see langword="true" />.
/// </summary>
/// <remarks>
///     GitLab's OpenAPI document declares these three creates and the poll with no response body at all,
///     even though every one of them answers with this entity. Every member except the id is therefore
///     nullable: the shape is documented rather than specified, and an instance that trims it must not
///     produce a deserialization failure.
/// </remarks>
public sealed record GitLabDependencyListExport
{
    public required long Id { get; init; }

    /// <summary>Whether the export has been generated and is ready to download.</summary>
    public bool? HasFinished { get; init; }

    /// <summary>The API route this export is polled on.</summary>
    public Uri? Self { get; init; }

    /// <summary>The API route the finished export file is downloaded from.</summary>
    public Uri? Download { get; init; }
}