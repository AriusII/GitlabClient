namespace GitLab.Client.Models;

/// <summary>
///     The answer to <c>GET /namespaces/:id/gitlab_subscription</c>. Every member is nullable: GitLab's
///     schema itself declares <see cref="Plan" />, <see cref="Usage" /> and <see cref="Billing" /> as
///     untyped objects with no <c>required</c> list, and a namespace with no active subscription answers
///     with some or all of them absent rather than as an error.
/// </summary>
public sealed record GitLabNamespaceSubscription
{
    public GitLabNamespaceSubscriptionPlan? Plan { get; init; }

    public GitLabNamespaceSubscriptionUsage? Usage { get; init; }

    public GitLabNamespaceSubscriptionBilling? Billing { get; init; }
}