namespace GitLab.Client.Models;

/// <summary>The billing-dates portion of <see cref="GitLabNamespaceSubscription" />.</summary>
public sealed record GitLabNamespaceSubscriptionBilling
{
    public DateOnly? SubscriptionStartDate { get; init; }

    public DateOnly? SubscriptionEndDate { get; init; }

    public DateOnly? TrialEndsOn { get; init; }
}