using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>One extra piece of context attached to a <c>POST /chat/completions</c> question.</summary>
public sealed record DuoChatAdditionalContext
{
    /// <summary>Where this context came from.</summary>
    public required DuoChatContextCategory Category { get; init; }

    /// <summary>What identifies it within its category - a file path, a merge request reference. At most 255 characters.</summary>
    public required string Id { get; init; }

    /// <summary>The context text itself. At most 1,000 characters.</summary>
    public required string Content { get; init; }

    /// <summary>
    ///     Anything else the client wants to attach. Untyped in the spec and interpreted per category, so
    ///     it is carried as a raw <see cref="JsonElement" /> rather than a shape GitLab does not promise.
    /// </summary>
    public JsonElement? Metadata { get; init; }
}