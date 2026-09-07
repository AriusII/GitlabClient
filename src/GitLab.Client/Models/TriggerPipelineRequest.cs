namespace GitLab.Client.Models;

/// <summary>
///     Request body for both pipeline-trigger endpoints (<c>POST /projects/:id/trigger/pipeline</c> and
///     <c>POST /projects/:id/ref/:ref/trigger/pipeline</c>) - the spec gives them the identical body.
/// </summary>
public sealed record TriggerPipelineRequest
{
    /// <summary>
    ///     A pipeline trigger token or a CI job token. These endpoints authenticate on this field rather than
    ///     on the configured <c>PRIVATE-TOKEN</c> header, so an empty value is a 400 that reads like a client
    ///     bug. With a job token the new pipeline becomes a multi-project pipeline linked to the upstream one.
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    ///     Variables injected into the pipeline, sent as a JSON object (<c>"variables": { "KEY": "value" }</c>) -
    ///     not the <c>variables[KEY]=value</c> form encoding GitLab's curl examples show.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Variables { get; init; }
}