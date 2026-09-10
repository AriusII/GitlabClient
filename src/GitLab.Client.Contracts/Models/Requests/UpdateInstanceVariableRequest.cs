namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PUT /admin/ci/variables/:key</c>.
///     <para>
///         Instance variables are scoped to the whole GitLab installation, therefore this contract has neither
///         <c>environment_scope</c> nor the project/group selector <c>filter</c>.
///     </para>
/// </summary>
public sealed record UpdateInstanceVariableRequest
{
    /// <summary>The replacement secret value.</summary>
    public string? Value { get; init; }

    /// <summary>Optional human-readable description of the variable.</summary>
    public string? Description { get; init; }

    public bool? Protected { get; init; }

    public bool? Masked { get; init; }

    /// <summary>When <see langword="true" />, GitLab does not expand <c>$VARIABLE</c> references.</summary>
    public bool? Raw { get; init; }

    /// <summary>Either <c>env_var</c> or <c>file</c>.</summary>
    public string? VariableType { get; init; }
}