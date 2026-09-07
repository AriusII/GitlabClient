namespace GitLab.Client.Models;

/// <summary>
///     Body of <c>PUT /projects/:id/debian_distributions/:codename</c> and
///     <c>PUT /groups/:id/-/debian_distributions/:codename</c>. The codename itself is not settable here
///     - it identifies the distribution in the route and is immutable once created.
/// </summary>
public sealed record UpdateDebianDistributionRequest
{
    public string? Suite { get; init; }

    public string? Origin { get; init; }

    public string? Label { get; init; }

    public string? Description { get; init; }

    /// <summary>How long a client should treat the generated <c>Release</c> file as fresh, in seconds.</summary>
    public int? ValidTimeDurationSeconds { get; init; }

    public IReadOnlyList<string>? Components { get; init; }

    public IReadOnlyList<string>? Architectures { get; init; }
}