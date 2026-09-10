using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     A CI/CD input value. GitLab's pipeline and pipeline-schedule schemas declare a closed union of a
///     string, number, Boolean, array, or explicit <see langword="null" />.
/// </summary>
/// <remarks>
///     The union is represented without <c>object</c> or <see cref="System.Text.Json.JsonElement" /> so
///     consumers retain a statically typed, Native-AOT-safe contract for every value shape the specification
///     declares. Construct a value with one of the named factory methods and inspect <see cref="Kind" />
///     before reading its corresponding value property.
/// </remarks>
[JsonConverter(typeof(GitLabPipelineInputValueConverter))]
public sealed record GitLabPipelineInputValue
{
    private GitLabPipelineInputValue(GitLabPipelineInputValueKind kind, string? textValue, double? numberValue,
        bool? flagValue, IReadOnlyList<GitLabPipelineInputValue>? sequenceValue)
    {
        Kind = kind;
        TextValue = textValue;
        NumberValue = numberValue;
        FlagValue = flagValue;
        SequenceValue = sequenceValue;
    }

    /// <summary>The active JSON shape.</summary>
    public GitLabPipelineInputValueKind Kind { get; }

    /// <summary>The value when <see cref="Kind" /> is <see cref="GitLabPipelineInputValueKind.Text" />.</summary>
    public string? TextValue { get; }

    /// <summary>The value when <see cref="Kind" /> is <see cref="GitLabPipelineInputValueKind.Number" />.</summary>
    public double? NumberValue { get; }

    /// <summary>The value when <see cref="Kind" /> is <see cref="GitLabPipelineInputValueKind.Flag" />.</summary>
    public bool? FlagValue { get; }

    /// <summary>The entries when <see cref="Kind" /> is <see cref="GitLabPipelineInputValueKind.Sequence" />.</summary>
    public IReadOnlyList<GitLabPipelineInputValue>? SequenceValue { get; }

    /// <summary>Creates an explicit JSON <see langword="null" /> value.</summary>
    public static GitLabPipelineInputValue Null()
    {
        return new GitLabPipelineInputValue(GitLabPipelineInputValueKind.Null, null, null, null, null);
    }

    /// <summary>Creates a JSON string value.</summary>
    public static GitLabPipelineInputValue Text(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new GitLabPipelineInputValue(GitLabPipelineInputValueKind.Text, value, null, null, null);
    }

    /// <summary>Creates a JSON number value.</summary>
    public static GitLabPipelineInputValue Number(double value)
    {
        return new GitLabPipelineInputValue(GitLabPipelineInputValueKind.Number, null, value, null, null);
    }

    /// <summary>Creates a JSON Boolean value.</summary>
    public static GitLabPipelineInputValue Flag(bool value)
    {
        return new GitLabPipelineInputValue(GitLabPipelineInputValueKind.Flag, null, null, value, null);
    }

    /// <summary>Creates a JSON array value.</summary>
    public static GitLabPipelineInputValue Sequence(IReadOnlyList<GitLabPipelineInputValue> value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new GitLabPipelineInputValue(GitLabPipelineInputValueKind.Sequence, null, null, null, value);
    }
}