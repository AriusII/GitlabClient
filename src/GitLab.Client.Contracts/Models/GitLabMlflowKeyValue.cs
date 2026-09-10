namespace GitLab.Client.Models;

/// <summary>
///     A single MLflow tag or run parameter - the <c>KeyValue</c> proto MLflow uses for
///     <c>tags</c> on an experiment, a run or a model version, and for the <c>params</c> of a run.
/// </summary>
/// <remarks>
///     MLflow models tags and parameters as the same two-field pair rather than as a JSON object, so a
///     key may legally repeat within one array; treat these as an ordered list, not a dictionary.
/// </remarks>
public sealed record GitLabMlflowKeyValue
{
    /// <summary>The tag or parameter name.</summary>
    public required string Key { get; init; }

    /// <summary>The value, always transported as a string - MLflow never types it.</summary>
    public string? Value { get; init; }
}