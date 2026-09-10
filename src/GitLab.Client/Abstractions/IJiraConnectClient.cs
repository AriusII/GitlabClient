using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the plumbing behind GitLab's Jira Cloud app - the endpoints under
///     <c>/integrations/jira_connect</c> and <c>/integrations/jira_forge</c> that link a GitLab namespace
///     to a Jira site so branches, commits and merge requests appear as development information on Jira
///     issues.
///     <para>
///         <b>Read this before reaching for any of it.</b> This is not the "Jira integration" most callers
///         mean. Configuring a project to talk to a Jira server - the URL, the credentials, the transition
///         ids - is the <c>jira</c> project integration and lives on the integrations surface
///         (<c>/projects/:id/integrations/jira</c>). The endpoints here belong to the other direction: the
///         GitLab app installed <i>into</i> Jira, which asks GitLab which namespaces it may read.
///     </para>
///     <para>
///         Consequently most of these are called by the Atlassian app itself, from inside its own iframe or
///         Forge invocation context, and not by an ordinary API consumer holding a personal access token.
///         Each method below says which. The two worth calling by hand are
///         <see cref="ListForgeSubscriptionsAsync" /> - to audit what a Forge installation can see - and
///         <see cref="DeleteForgeSubscriptionAsync" />, to revoke one.
///     </para>
///     <para>
///         Two generations of the app coexist. The older <b>Jira Connect</b> app authenticates each request
///         with an Atlassian-issued JWT; the newer <b>GitLab for Jira (Forge)</b> app authenticates as the
///         GitLab user and resolves the Jira installation from the Forge context, which is why its methods
///         take no token. Every <c>jira_forge</c> endpoint is marked
///         <c>x-gitlab-lifecycle: experiment</c> in the GitLab 19.4 spec, so treat its shape as less
///         settled than the rest of this library.
///     </para>
///     <para>
///         The writes all answer with the same contentless <see cref="GitLabJiraConnectResult" />
///         acknowledgement. Returning from one without an exception is the outcome; there is nothing to
///         inspect.
///     </para>
/// </summary>
public interface IJiraConnectClient
{
    /// <summary>
    ///     Subscribes a GitLab namespace to a Jira Connect installation
    ///     (<c>POST /integrations/jira_connect/subscriptions</c>).
    ///     <para>
    ///         Called by the Jira Connect app, not by a human: the request is authorized by
    ///         <see cref="SubscribeJiraConnectNamespaceRequest.Jwt" />, an Atlassian-issued token that names
    ///         the installation doing the asking. Without one there is nothing to subscribe the namespace
    ///         <i>to</i>, and GitLab answers <c>401</c>. For the Forge app use
    ///         <see cref="CreateForgeSubscriptionAsync" /> instead.
    ///     </para>
    /// </summary>
    /// <exception cref="Exceptions.GitLabForbiddenException">
    ///     The user the JWT resolves to may not administer the namespace. Subscribing requires owner rights
    ///     on it.
    /// </exception>
    Task<GitLabJiraConnectResult> SubscribeNamespaceAsync(SubscribeJiraConnectNamespaceRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Streams the GitLab namespaces subscribed to the GitLab for Jira (Forge) installation
    ///     (<c>GET /integrations/jira_forge/subscriptions</c>) - that is, everything the Jira site can
    ///     currently see development data for.
    ///     <para>
    ///         The scope is the installation the caller's credential resolves to, not a namespace you choose;
    ///         there are no filter or paging parameters. This is the one read on the resource, and the useful
    ///         one for auditing an installation's reach from outside the Jira app.
    ///     </para>
    /// </summary>
    IAsyncEnumerable<GitLabJiraConnectSubscription> ListForgeSubscriptionsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Subscribes a GitLab namespace to the GitLab for Jira (Forge) installation
    ///     (<c>POST /integrations/jira_forge/subscriptions</c>).
    ///     <para>
    ///         The Forge equivalent of <see cref="SubscribeNamespaceAsync" />, and the reason it takes no JWT:
    ///         the call authenticates as the GitLab user, while the Jira installation and Jira user come from
    ///         the Forge invocation context around it. Called by the app during its configuration flow.
    ///     </para>
    /// </summary>
    /// <exception cref="Exceptions.GitLabForbiddenException">
    ///     The authenticated user may not administer the namespace named in the request.
    /// </exception>
    Task<GitLabJiraConnectResult> CreateForgeSubscriptionAsync(CreateJiraForgeSubscriptionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Unsubscribes a GitLab namespace from the GitLab for Jira (Forge) installation
    ///     (<c>DELETE /integrations/jira_forge/subscriptions/:id</c>), cutting off the flow of its
    ///     development data to Jira.
    ///     <para>
    ///         The id is the subscription's own id, not the namespace's. Unlike most deletes in this library
    ///         the endpoint answers <c>200</c> with a body rather than <c>204</c>, so this returns a result.
    ///     </para>
    /// </summary>
    /// <param name="subscriptionId">
    ///     Id of the subscription to remove, as it appears in the GitLab web UI's unlink control - see
    ///     <see cref="GitLabJiraConnectSubscription.UnlinkPath" />.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<GitLabJiraConnectResult> DeleteForgeSubscriptionAsync(long subscriptionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Points the GitLab for Jira (Forge) installation at a GitLab instance
    ///     (<c>PUT /integrations/jira_forge/installation</c>).
    ///     <para>
    ///         This is how a Jira site installed from the Atlassian Marketplace is redirected at a self-managed
    ///         GitLab rather than GitLab.com. Leaving
    ///         <see cref="UpdateJiraForgeInstallationRequest.InstanceUrl" /> null points it back at GitLab.com.
    ///     </para>
    ///     <para>
    ///         Authorization is on the Jira side, not the GitLab side: the caller must be a Jira site or
    ///         organization administrator, which in practice means the Forge app making the call on their
    ///         behalf.
    ///     </para>
    /// </summary>
    /// <exception cref="Exceptions.GitLabValidationException">
    ///     The instance URL is malformed, longer than the 1024 characters GitLab accepts, or does not answer
    ///     as a GitLab instance.
    /// </exception>
    Task<GitLabJiraConnectResult> UpdateForgeInstallationAsync(UpdateJiraForgeInstallationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Registers the Forge app's system token with GitLab
    ///     (<c>POST /integrations/jira_forge/installation/forge_token</c>), so GitLab can push development
    ///     information straight to Jira instead of waiting to be polled.
    ///     <para>
    ///         Callable only by the Forge app itself, and the one method here whose inputs this library cannot
    ///         supply: GitLab reads the token from the <c>X-Forge-Oauth-System</c> request header and the Jira
    ///         <c>apiBaseUrl</c> from the Forge invocation token, neither of which is part of the request body
    ///         or of <see cref="IGitLabApiConnection" />'s outbound headers. From an ordinary API client the
    ///         call therefore has nothing to store and fails; it is wrapped for completeness of the surface.
    ///     </para>
    /// </summary>
    Task<GitLabJiraConnectResult> RegisterForgeTokenAsync(CancellationToken cancellationToken = default);
}