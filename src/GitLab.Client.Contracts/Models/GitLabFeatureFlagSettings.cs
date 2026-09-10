namespace GitLab.Client.Models;

/// <summary>
///     A project's feature flag settings (<c>/projects/:id/feature_flags_settings</c>).
/// </summary>
public sealed record GitLabFeatureFlagSettings
{
    /// <summary>
    ///     The minimum role that may create, update, toggle and delete this project's feature flags -
    ///     <c>no_one_allowed</c>, <c>developer</c>, <c>maintainer</c> or <c>owner</c>. The response schema
    ///     types this as a bare string with no enumeration, so it stays a string; the write side takes the
    ///     enumerated <see cref="GitLabMinimumRole" />, which the spec does enumerate there.
    /// </summary>
    public required string MinimumRole { get; init; }
}