namespace GitLab.Client.Models;

/// <summary>
///     A CI/CD variable, as returned by the project (<c>/projects/:id/variables</c>) and group
///     (<c>/groups/:id/variables</c>) variable endpoints. Both scopes return the identical shape.
/// </summary>
public sealed record GitLabVariable
{
    public required string Key { get; init; }

    /// <summary>
    ///     Null for a variable created with <c>masked_and_hidden</c>: GitLab never returns the value of a
    ///     hidden variable again, not even to the user who created it.
    /// </summary>
    public string? Value { get; init; }

    /// <summary>Either <c>env_var</c> or <c>file</c>.</summary>
    public string? VariableType { get; init; }

    public bool? Protected { get; init; }

    public bool? Masked { get; init; }

    public bool? Hidden { get; init; }

    /// <summary>When true, GitLab does not expand <c>$VARIABLE</c> references inside the value.</summary>
    public bool? Raw { get; init; }

    /// <summary>The environment this variable applies to, or <c>*</c> for all of them.</summary>
    public string? EnvironmentScope { get; init; }

    public string? Description { get; init; }
}