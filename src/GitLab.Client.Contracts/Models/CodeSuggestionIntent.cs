using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>What a <c>POST /code_suggestions/completions</c> call is asking the model to do.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<CodeSuggestionIntent>))]
public enum CodeSuggestionIntent
{
    /// <summary>Finish the statement the cursor sits in.</summary>
    [JsonStringEnumMemberName("completion")]
    Completion,

    /// <summary>Write a larger block from scratch - see <see cref="CodeSuggestionGenerationType" />.</summary>
    [JsonStringEnumMemberName("generation")]
    Generation
}