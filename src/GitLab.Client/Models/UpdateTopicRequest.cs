namespace GitLab.Client.Models;

/// <summary>
///     Body of <c>PUT /topics/:id</c>, which updates a project topic. Administrators only. Every field is
///     optional; anything left null is omitted from the request and keeps its current value.
/// </summary>
/// <remarks>
///     As on create, the avatar is not a JSON field - GitLab takes it as a <c>multipart/form-data</c> part,
///     which is what <c>ITopicsClient.SetAvatarAsync</c> sends.
/// </remarks>
public sealed record UpdateTopicRequest
{
    /// <summary>The new topic slug.</summary>
    public string? Name { get; init; }

    /// <summary>The new human-readable title.</summary>
    public string? Title { get; init; }

    /// <summary>The new description.</summary>
    public string? Description { get; init; }
}