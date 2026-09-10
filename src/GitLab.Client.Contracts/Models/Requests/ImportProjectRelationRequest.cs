namespace GitLab.Client.Models.Requests;

/// <summary>
///     The non-file half of <c>POST /projects/import-relation</c>, which replays one relation out of a
///     project export archive into an existing project. The archive is passed separately, as the upload's
///     file part.
/// </summary>
/// <remarks>
///     Projected onto multipart form fields rather than serialized as a JSON body; see
///     <see cref="ImportProjectArchiveRequest" /> for why it is still registered with the serializer.
/// </remarks>
public sealed record ImportProjectRelationRequest
{
    /// <summary>The full path of the existing project to import into.</summary>
    public required string Path { get; init; }

    /// <summary>
    ///     Which relation to extract from the archive. GitLab accepts <c>issues</c>,
    ///     <c>merge_requests</c>, <c>ci_pipelines</c> and <c>milestones</c>; items already imported are
    ///     skipped.
    /// </summary>
    public required string Relation { get; init; }
}