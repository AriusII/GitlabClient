namespace GitLab.Client.Models;

/// <summary>
///     One parameter in a <see cref="LogMlflowBatchRequest" />. Distinct from
///     <see cref="GitLabMlflowKeyValue" />, the read-side pair: here both members are required, because
///     GitLab rejects a batch entry that omits either.
/// </summary>
public sealed record MlflowParameterEntry
{
    /// <summary>The parameter name.</summary>
    public required string Key { get; init; }

    /// <summary>The parameter value. MLflow transports every parameter as a string.</summary>
    public required string Value { get; init; }
}