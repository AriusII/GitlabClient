using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>
///     What triggered a <see cref="CodeSuggestionIntent.Generation" /> request, which decides how the AI
///     Gateway prompts the model.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<CodeSuggestionGenerationType>))]
public enum CodeSuggestionGenerationType
{
    /// <summary>The cursor follows a comment describing what to write.</summary>
    [JsonStringEnumMemberName("comment")] Comment,

    /// <summary>The cursor sits in a function with an empty body.</summary>
    [JsonStringEnumMemberName("empty_function")]
    EmptyFunction,

    /// <summary>The whole file is short enough to be treated as the prompt.</summary>
    [JsonStringEnumMemberName("small_file")]
    SmallFile
}