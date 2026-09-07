namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /hooks/:hook_id/url_variables/:key</c> - the variable name travels in the
///     route, this carries only its replacement value.
///     <para>
///         Treat an instance of this type as a credential: URL variables exist so a secret can be kept out
///         of the readable hook URL. Do not log it, do not put it in an exception message, and do not hand
///         it to a generic object dumper. <see cref="ToString" /> is overridden here for exactly that
///         reason.
///     </para>
/// </summary>
public sealed record UpdateSystemHookUrlVariableRequest
{
    public required string Value { get; init; }

    /// <summary>Renders the request without its secret value.</summary>
    public override string ToString()
    {
        return "UpdateSystemHookUrlVariableRequest { Value = <redacted> }";
    }
}