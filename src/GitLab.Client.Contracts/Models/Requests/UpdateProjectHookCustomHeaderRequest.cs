namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PUT /projects/:id/hooks/:hook_id/custom_headers/:key</c> - the header name
///     travels in the route, this carries only its replacement value.
///     <para>
///         Treat an instance of this type as a credential: custom header values routinely carry
///         authentication material. Do not log it, do not put it in an exception message, and do not hand
///         it to a generic object dumper. <see cref="ToString" /> is overridden here for exactly that
///         reason.
///     </para>
/// </summary>
public sealed record UpdateProjectHookCustomHeaderRequest
{
    public required string Value { get; init; }

    /// <summary>Renders the request without its secret value.</summary>
    public override string ToString()
    {
        return "UpdateProjectHookCustomHeaderRequest { Value = <redacted> }";
    }
}