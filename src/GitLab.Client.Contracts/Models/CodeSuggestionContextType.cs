using System.Text.Json.Serialization;

using GitLab.Client.Models.Requests;

namespace GitLab.Client.Models;

/// <summary>What one entry of <see cref="GenerateCodeCompletionRequest.Context" /> carries.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<CodeSuggestionContextType>))]
public enum CodeSuggestionContextType
{
    /// <summary>A whole related file.</summary>
    [JsonStringEnumMemberName("file")] File,

    /// <summary>An excerpt rather than a whole file.</summary>
    [JsonStringEnumMemberName("snippet")] Snippet
}