namespace GitLab.Client.Models;

/// <summary>
///     The state of one relation's export for a project
///     (<c>GET /projects/:id/export_relations/status</c>). Relations exports are the "direct transfer"
///     export format: one NDJSON file per relation, rather than the single archive that
///     <c>/projects/:id/export</c> produces.
/// </summary>
public sealed record GitLabProjectRelationExportStatus
{
    /// <summary>The relation this entry describes - <c>issues</c>, <c>merge_requests</c>, <c>milestones</c>.</summary>
    public string? Relation { get; init; }

    /// <summary>How far along this relation's export is.</summary>
    public GitLabProjectRelationExportState? Status { get; init; }

    /// <summary>The failure message when <see cref="Status" /> is <see cref="GitLabProjectRelationExportState.Failed" />.</summary>
    public string? Error { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>Whether the relation was exported in batches rather than as a single file.</summary>
    public bool? Batched { get; init; }

    /// <summary>How many batches a batched export produced.</summary>
    public int? BatchesCount { get; init; }

    /// <summary>How many records the relation holds in total, across every batch.</summary>
    public int? TotalObjectsCount { get; init; }

    /// <summary>
    ///     The individual batches of a batched export. Null or empty when <see cref="Batched" /> is false.
    ///     <para>
    ///         Modelled as a list because GitLab exposes a has-many association here; the OpenAPI document
    ///         describes the member with a single-object <c>$ref</c>, which is an artefact of how the spec is
    ///         generated from Grape entities rather than the shape on the wire.
    ///     </para>
    /// </summary>
    public IReadOnlyList<GitLabProjectRelationExportBatch>? Batches { get; init; }
}