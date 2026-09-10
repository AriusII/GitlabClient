namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for adding an emoji reaction (<c>POST .../award_emoji</c>).
/// </summary>
public sealed record CreateAwardEmojiRequest
{
    /// <summary>
    ///     Name of the emoji <em>without</em> surrounding colons - <c>thumbsup</c>, not <c>:thumbsup:</c>.
    ///     GitLab rejects the colon-wrapped form.
    /// </summary>
    public required string Name { get; init; }
}