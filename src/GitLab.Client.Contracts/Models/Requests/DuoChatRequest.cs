using System.Text.Json;

namespace GitLab.Client.Models.Requests;

/// <summary>Body of <c>POST /chat/completions</c> - one turn of a GitLab Duo Chat conversation.</summary>
public sealed record DuoChatRequest
{
    /// <summary>The user's question. At most 1,000 characters, and the only member GitLab requires.</summary>
    public required string Content { get; init; }

    /// <summary>The kind of object the question is about, if any.</summary>
    public DuoChatResourceType? ResourceType { get; init; }

    /// <summary>
    ///     Which object of that kind - a numeric id for most types, a commit SHA for
    ///     <see cref="DuoChatResourceType.Commit" />. GitLab types this as either a number or a string, so
    ///     it is a raw <see cref="JsonElement" /> rather than a guess at one of them; build one with
    ///     <c>JsonDocument.Parse("42").RootElement.Clone()</c>.
    /// </summary>
    public JsonElement? ResourceId { get; init; }

    /// <summary>
    ///     The page the question was asked from - a GitLab issue or merge request URL - used for telemetry
    ///     and to give the answer a location. At most 1,000 characters.
    /// </summary>
    public Uri? RefererUrl { get; init; }

    /// <summary>Correlates the eventual streamed answer back to this client. At most 500 characters.</summary>
    public string? ClientSubscriptionId { get; init; }

    /// <summary>Clear the conversation history before and after this turn, so the question stands alone.</summary>
    public bool? WithCleanHistory { get; init; }

    /// <summary>
    ///     The project the resource belongs to. Required when <see cref="ResourceType" /> is
    ///     <see cref="DuoChatResourceType.Commit" />, because a commit SHA does not identify a project on
    ///     its own.
    /// </summary>
    public long? ProjectId { get; init; }

    /// <summary>The buffer the user is looking at, if the question comes from an IDE.</summary>
    public DuoChatCurrentFile? CurrentFile { get; init; }

    /// <summary>Extra context - related files, issues, terminal output - to answer against.</summary>
    public IReadOnlyList<DuoChatAdditionalContext>? AdditionalContext { get; init; }
}