namespace GitLab.Client.Models;

/// <summary>The plan portion of <see cref="GitLabNamespaceSubscription" />.</summary>
public sealed record GitLabNamespaceSubscriptionPlan
{
    /// <summary>The plan's machine name ("ultimate", "premium", ...).</summary>
    public string? Code { get; init; }

    public string? Name { get; init; }

    public bool? Trial { get; init; }

    public bool? AutoRenew { get; init; }

    public bool? Upgradable { get; init; }

    public bool? ExcludeGuests { get; init; }
}