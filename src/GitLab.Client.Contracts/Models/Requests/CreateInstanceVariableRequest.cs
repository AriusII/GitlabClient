namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>POST /admin/ci/variables</c>.
///     <para>
///         This intentionally does not reuse <see cref="CreateVariableRequest" />. Instance variables have
///         no environment scope and GitLab's GitLab 19.4 schema does not accept <c>masked_and_hidden</c> at
///         this endpoint. Keeping those properties out of this wire contract prevents a caller from sending
///         project-only fields that an instance administrator cannot use.
///     </para>
/// </summary>
public sealed record CreateInstanceVariableRequest
{
    /// <summary>The variable name. GitLab accepts letters, digits, and underscores.</summary>
    public required string Key { get; init; }

    /// <summary>The secret value stored at the instance scope.</summary>
    public required string Value { get; init; }

    /// <summary>Optional human-readable description of the variable.</summary>
    public string? Description { get; init; }

    public bool? Protected { get; init; }

    public bool? Masked { get; init; }

    /// <summary>When <see langword="true" />, GitLab does not expand <c>$VARIABLE</c> references.</summary>
    public bool? Raw { get; init; }

    /// <summary>Either <c>env_var</c> (the default) or <c>file</c>.</summary>
    public string? VariableType { get; init; }
}