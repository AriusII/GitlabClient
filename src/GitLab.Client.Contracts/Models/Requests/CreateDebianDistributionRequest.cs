namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body of <c>POST /projects/:id/debian_distributions</c> and
///     <c>POST /groups/:id/-/debian_distributions</c>.
/// </summary>
public sealed record CreateDebianDistributionRequest
{
    /// <summary>
    ///     The Debian codename. The only field GitLab requires to create a distribution, and the value
    ///     every subsequent route addresses it by.
    /// </summary>
    public required string Codename { get; init; }

    public string? Suite { get; init; }

    public string? Origin { get; init; }

    public string? Label { get; init; }

    public string? Description { get; init; }

    /// <summary>How long a client should treat the generated <c>Release</c> file as fresh, in seconds.</summary>
    public int? ValidTimeDurationSeconds { get; init; }

    public IReadOnlyList<string>? Components { get; init; }

    public IReadOnlyList<string>? Architectures { get; init; }
}