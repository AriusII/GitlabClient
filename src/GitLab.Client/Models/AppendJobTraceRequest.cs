namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PATCH /jobs/:id/trace</c>, as captured by the spec. Part of the runner
///     protocol; see <see cref="JobRequestRequest" />.
///     <para>
///         The spec's <c>params</c> block for this operation carries only these two fields - the actual
///         trace bytes being appended are read straight off the raw PATCH body by GitLab's controller,
///         outside Grape's parameter parsing, and never appear as a named property anywhere in the
///         OpenAPI document. This request type is therefore necessarily incomplete: it lets a caller set
///         <see cref="Token" /> and <see cref="DebugTrace" />, but cannot carry the log text itself,
///         because the transport this library builds on has no raw-body PATCH overload to carry it.
///     </para>
/// </summary>
public sealed record AppendJobTraceRequest
{
    /// <summary>The job's authentication token.</summary>
    public string? Token { get; init; }

    /// <summary>Turns the debug trace on or off for the rest of the job.</summary>
    public bool? DebugTrace { get; init; }
}