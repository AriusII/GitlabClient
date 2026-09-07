namespace GitLab.Client.Models;

/// <summary>
///     The trimmed user shape GitLab embeds as the <c>uploaded_by</c> of a project or group upload.
///     <para>
///         Deliberately not <see cref="GitLabUser" />: the spec models this as its own "user safe" entity
///         carrying only the four members below, so reusing the fuller shape would fail deserialization on
///         its required <see cref="GitLabUser.WebUrl" />.
///     </para>
/// </summary>
public sealed record GitLabUploadAuthor
{
    public required long Id { get; init; }

    public required string Username { get; init; }

    public string? Name { get; init; }

    /// <summary>The user's public email, when they publish one. Null - not omitted - for most users.</summary>
    public string? PublicEmail { get; init; }
}