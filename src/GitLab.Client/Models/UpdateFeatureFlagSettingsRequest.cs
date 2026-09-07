namespace GitLab.Client.Models;

/// <summary>Body of <c>PUT /projects/:id/feature_flags_settings</c>.</summary>
public sealed record UpdateFeatureFlagSettingsRequest
{
    /// <summary>
    ///     The minimum role allowed to create, update, toggle and delete the project's feature flags. The
    ///     spec enumerates this vocabulary on the write side, so it is an enum here even though
    ///     <see cref="GitLabFeatureFlagSettings.MinimumRole" /> reads it back as a bare string.
    /// </summary>
    public required GitLabMinimumRole MinimumRole { get; init; }
}