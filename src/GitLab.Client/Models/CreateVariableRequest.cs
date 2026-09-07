namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects/:id/variables</c> and <c>POST /groups/:id/variables</c> - the
///     spec gives the two endpoints the identical body.
/// </summary>
public sealed record CreateVariableRequest
{
    public required string Key { get; init; }

    public required string Value { get; init; }

    public bool? Protected { get; init; }

    public bool? Masked { get; init; }

    /// <summary>
    ///     Masks the value <em>and</em> hides it from every later read. GitLab returns the created variable
    ///     with a null <c>value</c> and never discloses it again, so store it before sending this.
    /// </summary>
    public bool? MaskedAndHidden { get; init; }

    /// <summary>When true, GitLab does not expand <c>$VARIABLE</c> references inside the value.</summary>
    public bool? Raw { get; init; }

    /// <summary>Either <c>env_var</c> (the default) or <c>file</c>.</summary>
    public string? VariableType { get; init; }

    /// <summary>
    ///     The environment this variable applies to; defaults to <c>*</c>. Reusing an existing key requires a
    ///     different scope, otherwise GitLab answers 400 "VARIABLE_NAME has already been taken".
    /// </summary>
    public string? EnvironmentScope { get; init; }

    public string? Description { get; init; }
}