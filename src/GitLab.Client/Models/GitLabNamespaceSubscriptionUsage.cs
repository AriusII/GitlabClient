namespace GitLab.Client.Models;

/// <summary>The seat-usage portion of <see cref="GitLabNamespaceSubscription" />.</summary>
public sealed record GitLabNamespaceSubscriptionUsage
{
    public int? SeatsInSubscription { get; init; }

    public int? SeatsInUse { get; init; }

    public int? MaxSeatsUsed { get; init; }

    public int? SeatsOwed { get; init; }
}