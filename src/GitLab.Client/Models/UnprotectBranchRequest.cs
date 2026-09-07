namespace GitLab.Client.Models;

/// <summary>
///     The empty body sent with <c>PUT /projects/:id/repository/branches/:branch/unprotect</c>.
/// </summary>
/// <remarks>
///     The endpoint declares no parameters but does answer with the updated branch, and the transport has
///     no body-less <c>PUT</c> that deserializes a response. Serializing this record yields an empty JSON
///     object, which Grape parses to an empty parameter set - the same thing a body-less PUT would
///     produce. It is internal because it is an artefact of the transport, not part of the public request
///     vocabulary.
/// </remarks>
internal sealed record UnprotectBranchRequest
{
    /// <summary>The single shared instance; the record carries no state worth allocating twice.</summary>
    public static UnprotectBranchRequest Instance { get; } = new();
}