namespace GitLab.Client.Models;

/// <summary>
///     The tracker GitLab returns for a single-relation import
///     (<c>POST /projects/import-relation</c>) - one relation extracted out of a project export archive
///     and replayed into an existing project.
/// </summary>
public sealed record GitLabProjectRelationImport
{
    public required long Id { get; init; }

    /// <summary>The full path of the project the relation is being imported into.</summary>
    public string? ProjectPath { get; init; }

    /// <summary>The relation being imported - <c>issues</c>, <c>merge_requests</c>, <c>ci_pipelines</c> or <c>milestones</c>.</summary>
    public string? Relation { get; init; }

    /// <summary>
    ///     Where the relation import has got to - <c>created</c>, <c>pending</c>, <c>started</c>,
    ///     <c>finished</c>, <c>failed</c>. Kept as a <see cref="string" /> because the spec types it as a
    ///     bare string with no enumeration.
    /// </summary>
    public string? Status { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}