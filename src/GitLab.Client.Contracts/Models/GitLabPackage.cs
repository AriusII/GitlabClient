using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     One package in the cross-format package registry summary - the shape GitLab returns from
///     <c>GET /groups/:id/packages</c>, <c>GET /projects/:id/packages</c> and
///     <c>GET /projects/:id/packages/:package_id</c>. Unlike the per-format registries (Conan, npm,
///     NuGet, ...), this view spans every package format a project or group holds and carries no
///     format-specific metadata of its own.
/// </summary>
public sealed record GitLabPackage
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    /// <summary>Set only for a Conan package, whose recipe name can differ from <see cref="Name" />.</summary>
    public string? ConanPackageName { get; init; }

    public string? Version { get; init; }

    public GitLabPackageType? PackageType { get; init; }

    public GitLabPackageStatus? Status { get; init; }

    [JsonPropertyName("_links")] public GitLabPackageLinks? Links { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? LastDownloadedAt { get; init; }

    /// <summary>ID of the user who created the package.</summary>
    public long? CreatorId { get; init; }

    public long? ProjectId { get; init; }

    /// <summary>The owning project's full path. Populated by the group-level listing; absent from a project's own.</summary>
    public string? ProjectPath { get; init; }

    public string? Tags { get; init; }

    /// <summary>
    ///     The pipeline that built this package, when GitLab can attribute one. Reused from the Pipelines
    ///     resource rather than an invented duplicate, since every field GitLab's
    ///     <c>APIEntitiesPackagePipeline</c> schema sends is already on <see cref="GitLabPipeline" />.
    /// </summary>
    public GitLabPipeline? Pipeline { get; init; }

    /// <summary>
    ///     Named <c>pipelines</c> on the wire despite carrying a single object, not a list - the same quirk
    ///     already documented on <see cref="GitLabPackageFile.Pipelines" /> for the sibling entity.
    /// </summary>
    public GitLabPipeline? Pipelines { get; init; }

    /// <summary>
    ///     Named <c>versions</c> on the wire despite GitLab's own schema declaring it as a single
    ///     <see cref="GitLabPackageVersion" /> object rather than an array.
    /// </summary>
    public GitLabPackageVersion? Versions { get; init; }
}