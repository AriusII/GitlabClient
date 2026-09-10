namespace GitLab.Client.Models.Requests;

/// <summary>
///     One group or project to migrate, as supplied in the <c>entities</c> array of
///     <c>POST /bulk_imports</c>.
/// </summary>
/// <remarks>
///     The spec's <c>destination_name</c> parameter is deliberately not surfaced: GitLab marks it
///     deprecated in favour of <see cref="DestinationSlug" />, which it reads first when both are sent,
///     so exposing it would only add a second way to say the same thing.
/// </remarks>
public sealed record BulkImportEntityRequest
{
    /// <summary>Whether this entry migrates a group or a project.</summary>
    public required GitLabBulkImportEntitySourceType SourceType { get; init; }

    /// <summary>
    ///     The entity's path on the source instance - <c>source/full/path</c>, not
    ///     <c>https://example.com/source/full/path</c>.
    /// </summary>
    public required string SourceFullPath { get; init; }

    /// <summary>
    ///     The namespace on this instance to create the entity in - <c>destination</c> or
    ///     <c>destination/namespace</c>.
    /// </summary>
    public required string DestinationNamespace { get; init; }

    /// <summary>
    ///     The last path component to create the entity under - <c>destination_slug</c>, never
    ///     <c>destination/slug</c>.
    /// </summary>
    public string? DestinationSlug { get; init; }

    /// <summary>
    ///     Whether a group migration also migrates the projects nested under it. GitLab defaults this to
    ///     true.
    /// </summary>
    public bool? MigrateProjects { get; init; }

    /// <summary>Whether memberships are carried across. GitLab defaults this to true.</summary>
    public bool? MigrateMemberships { get; init; }
}