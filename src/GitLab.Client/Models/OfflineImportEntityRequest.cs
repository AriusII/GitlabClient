namespace GitLab.Client.Models;

/// <summary>
///     One group or project to read back out of an offline transfer export. Narrower than
///     <see cref="BulkImportEntityRequest" />: an offline import has no source instance to talk to, so
///     there is nothing to migrate memberships or nested projects <em>from</em> beyond what the export
///     already contains.
/// </summary>
public sealed record OfflineImportEntityRequest
{
    /// <summary>Whether this entry imports a group or a project.</summary>
    public required GitLabBulkImportEntitySourceType SourceType { get; init; }

    /// <summary>The entity's full path on the source instance, as recorded in the export.</summary>
    public string? SourceFullPath { get; init; }

    /// <summary>The full path of the namespace on this instance to create the entity in.</summary>
    public string? DestinationNamespace { get; init; }

    /// <summary>The last path component to create the entity under.</summary>
    public string? DestinationSlug { get; init; }
}