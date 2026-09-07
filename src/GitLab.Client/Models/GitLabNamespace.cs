namespace GitLab.Client.Models;

/// <summary>
///     A GitLab namespace - the umbrella entity GitLab returns for both groups and personal (user)
///     namespaces (<c>GET /namespaces</c>, <c>GET /namespaces/:id</c>). <see cref="Kind" /> tells the two
///     apart ("group" or "user"); GitLab does not enumerate the value in its schema, so it stays a bare
///     string here rather than a real enum in case a future kind is ever added.
///     <para>
///         Everything past the first handful of members is nullable on purpose: the billing and quota
///         fields (<see cref="Plan" />, <see cref="SeatsInUse" />, <see cref="Trial" />, ...) are
///         populated only for a top-level namespace whose subscription exposes them, and are silently
///         absent everywhere else - including every embedding of this same shape inside another
///         resource's response.
///     </para>
/// </summary>
public sealed record GitLabNamespace
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public string? Path { get; init; }

    /// <summary>"group" or "user" - which kind of namespace this is.</summary>
    public string? Kind { get; init; }

    public string? FullPath { get; init; }

    /// <summary>The parent group's ID, or <see langword="null" /> for a top-level namespace.</summary>
    public long? ParentId { get; init; }

    public Uri? AvatarUrl { get; init; }

    public Uri? WebUrl { get; init; }

    public int? MembersCountWithDescendants { get; init; }

    public long? RootRepositorySize { get; init; }

    public int? ProjectsCount { get; init; }

    public int? SharedRunnersMinutesLimit { get; init; }

    public int? ExtraSharedRunnersMinutesLimit { get; init; }

    public long? AdditionalPurchasedStorageSize { get; init; }

    public DateOnly? AdditionalPurchasedStorageEndsOn { get; init; }

    public int? BillableMembersCount { get; init; }

    public int? SeatsInUse { get; init; }

    public int? MaxSeatsUsed { get; init; }

    public DateOnly? MaxSeatsUsedChangedAt { get; init; }

    /// <summary>The subscription's end date, for a namespace on a paid plan.</summary>
    public DateOnly? EndDate { get; init; }

    /// <summary>
    ///     The subscription plan name ("default", "premium", "ultimate", ...). GitLab's schema types this
    ///     as a bare string with no enumerated vocabulary, so it stays a <see cref="string" /> rather than a
    ///     real enum - a plan GitLab adds later must not turn a healthy response into a
    ///     <see cref="System.Text.Json.JsonException" />.
    /// </summary>
    public string? Plan { get; init; }

    public DateOnly? TrialEndsOn { get; init; }

    public bool? Trial { get; init; }
}