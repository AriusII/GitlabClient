namespace GitLab.Client.Models.Requests;

/// <summary>Body of <c>POST /topics</c>, which creates a project topic. Administrators only.</summary>
/// <remarks>
///     The avatar is deliberately absent: GitLab models it as a <c>multipart/form-data</c> file part, not a
///     JSON field, so it is set in a second call to
///     <c>ITopicsClient.SetAvatarAsync</c> once the topic exists.
/// </remarks>
public sealed record CreateTopicRequest
{
    /// <summary>The topic slug. Required, and unique across the instance.</summary>
    public required string Name { get; init; }

    /// <summary>The human-readable title shown in the UI. Required by GitLab, unlike on update.</summary>
    public required string Title { get; init; }

    /// <summary>Optional description shown on the topic's page.</summary>
    public string? Description { get; init; }

    /// <summary>The organization to create the topic in, on instances where organizations are enabled.</summary>
    public long? OrganizationId { get; init; }
}