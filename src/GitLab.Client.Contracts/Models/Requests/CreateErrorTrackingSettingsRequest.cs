namespace GitLab.Client.Models.Requests;

/// <summary>
///     Request body for <c>PUT /projects/:id/error_tracking/settings</c>. Requires the Maintainer or Owner
///     role on the project. GitLab requires both members here, unlike the
///     <see cref="UpdateErrorTrackingSettingsRequest" />
///     sent to the <c>PATCH</c> sibling, which only requires <see cref="UpdateErrorTrackingSettingsRequest.Active" />.
/// </summary>
public sealed record CreateErrorTrackingSettingsRequest
{
    /// <summary>
    ///     <see langword="true" /> to enable the configured Error Tracking settings, <see langword="false" /> to disable
    ///     them.
    /// </summary>
    public required bool Active { get; init; }

    /// <summary><see langword="true" /> to use GitLab's integrated Error Tracking backend instead of external Sentry.</summary>
    public required bool Integrated { get; init; }
}