namespace GitLab.Client.Models;

/// <summary>
///     A single MLflow key/value tag attached to a registered model or to one of its versions
///     (the spec's <c>APIEntitiesMlMlflowKeyValue</c>).
/// </summary>
/// <remarks>
///     MLflow models tags as free-form user metadata, so neither the key nor the value comes from a
///     fixed vocabulary.
/// </remarks>
public sealed record GitLabMlModelTag
{
    /// <summary>The tag name, unique within the model or version it is attached to.</summary>
    public required string Key { get; init; }

    /// <summary>The tag value. MLflow permits an empty value, and GitLab may omit the member entirely.</summary>
    public string? Value { get; init; }
}