namespace GitLab.Client.Models;

/// <summary>
///     One entry in the <c>versions</c> field of a <see cref="GitLabPackage" /> - GitLab's schema types
///     <see cref="Id" /> as a bare string here, unlike the numeric package ID elsewhere in this same
///     response, and leaves <see cref="CreatedAt" /> without a declared date-time format, so both stay
///     plain strings rather than risk a <see cref="System.Text.Json.JsonException" /> on a shape this
///     library cannot verify from the spec.
/// </summary>
public sealed record GitLabPackageVersion
{
    public string? Id { get; init; }

    public string? Version { get; init; }

    public string? CreatedAt { get; init; }

    public string? Tags { get; init; }

    /// <summary>
    ///     The pipeline that built this version, when GitLab can attribute one. Reused from the Pipelines
    ///     resource rather than an invented duplicate, since every field GitLab's
    ///     <c>APIEntitiesPackagePipeline</c> schema sends is already on <see cref="GitLabPipeline" />.
    /// </summary>
    public GitLabPipeline? Pipeline { get; init; }
}