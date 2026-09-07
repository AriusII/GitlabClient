namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>PATCH /projects/:id/error_tracking/settings</c>. Requires the Maintainer or
///     Owner role on the project.
/// </summary>
public sealed record UpdateErrorTrackingSettingsRequest
{
    /// <summary>
    ///     <see langword="true" /> to enable the already configured Error Tracking settings, <see langword="false" /> to
    ///     disable them.
    /// </summary>
    public required bool Active { get; init; }

    /// <summary>
    ///     <see langword="true" /> to enable GitLab's integrated Error Tracking backend. Available in GitLab 14.2 and
    ///     later.
    /// </summary>
    public bool? Integrated { get; init; }
}