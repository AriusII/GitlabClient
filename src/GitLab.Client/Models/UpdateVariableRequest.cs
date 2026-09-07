namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PUT /projects/:id/variables/:key</c> and <c>PUT /groups/:id/variables/:key</c>.
///     Neither form accepts <c>masked_and_hidden</c>: hiding a variable is a create-time decision.
/// </summary>
public sealed record UpdateVariableRequest
{
    public string? Value { get; init; }

    public bool? Protected { get; init; }

    public bool? Masked { get; init; }

    /// <summary>When true, GitLab does not expand <c>$VARIABLE</c> references inside the value.</summary>
    public bool? Raw { get; init; }

    /// <summary>Either <c>env_var</c> or <c>file</c>.</summary>
    public string? VariableType { get; init; }

    public string? EnvironmentScope { get; init; }

    public string? Description { get; init; }
}