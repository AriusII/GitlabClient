using System.Text.Json;

using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps four small, otherwise-unrelated GitLab OpenAPI tags that share nothing but their size:
///     <b>Agents</b> (<c>/job/allowed_agents</c>), <b>Authn</b> (<c>/iam/userinfo</c>),
///     <b>Token exchange</b> (<c>/token_exchange</c>) and <b>Project Google Cloud integration</b>
///     (<c>/projects/:id/google_cloud/setup/*</c>). None of the four contributes enough operations to
///     justify its own four-file client chain, so they are combined here rather than left unwrapped.
///     <para>
///         Every member of this client is young GitLab surface - cloud identity federation and
///         workload-identity token exchange - and three of the four underlying operations carry
///         <c>x-gitlab-lifecycle: experiment</c> in the spec. Expect route shapes, response fields and
///         even whether an operation survives to change faster here than on the rest of this package's
///         surface.
///     </para>
/// </summary>
public interface IPlatformIntegrationsClient
{
    /// <summary>
    ///     Lists the GitLab agents for Kubernetes available to the calling CI/CD job token
    ///     (<c>GET /job/allowed_agents</c>). Only meaningful when the client is authenticated with a job
    ///     token.
    /// </summary>
    /// <remarks>
    ///     GitLab's OpenAPI spec documents the response as the CI job entity the token belongs to
    ///     (<c>APIEntitiesCiJob</c>) rather than a list of agents, despite the operation's summary - this
    ///     wrapper follows the spec's declared schema rather than the summary text, so it returns
    ///     <see cref="GitLabJob" />. Verify against a live instance if the two keep disagreeing.
    /// </remarks>
    Task<GitLabJob> GetAllowedAgentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the OIDC ID-token claims for the caller, authenticated exclusively via an IAM-issued
    ///     OAuth JWT (<c>GET /iam/userinfo</c>).
    /// </summary>
    /// <remarks>
    ///     GitLab's spec declares no response schema for this operation, so the claims are surfaced as a
    ///     raw <see cref="JsonElement" /> rather than a shape the spec does not promise.
    /// </remarks>
    Task<JsonElement> GetIamUserInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the shell script that sets up a project's Google Cloud integration
    ///     (<c>GET /projects/:id/google_cloud/setup/integrations.sh</c>), streamed as plain text rather
    ///     than parsed.
    /// </summary>
    /// <param name="projectId">The project to generate the script for.</param>
    /// <param name="options">Which optional integrations the generated script should also enable.</param>
    /// <param name="cancellationToken">Cancels the request, and any read from the returned stream.</param>
    /// <returns>
    ///     The script body plus its media type. The caller owns it and must dispose it -
    ///     <c>await using</c> - once the body has been read.
    /// </returns>
    Task<GitLabFileResponse> GetGoogleCloudIntegrationSetupScriptAsync(ProjectId projectId,
        GoogleCloudIntegrationSetupScriptOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the shell script that configures a Google Cloud project for GitLab runner
    ///     provisioning (<c>GET /projects/:id/google_cloud/setup/runner_deployment_project.sh</c>), streamed
    ///     as plain text rather than parsed.
    /// </summary>
    /// <param name="projectId">The project to generate the script for.</param>
    /// <param name="googleCloudProjectId">The Google Cloud project the runners should be provisioned into.</param>
    /// <param name="cancellationToken">Cancels the request, and any read from the returned stream.</param>
    /// <returns>
    ///     The script body plus its media type. The caller owns it and must dispose it -
    ///     <c>await using</c> - once the body has been read.
    /// </returns>
    Task<GitLabFileResponse> GetGoogleCloudRunnerDeploymentSetupScriptAsync(ProjectId projectId,
        string googleCloudProjectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Issues a short-lived JWT scoped to a single modular-service audience, such as the Artifact
    ///     Registry (<c>POST /token_exchange</c>).
    /// </summary>
    /// <param name="request">The audience to scope the token to, and its requested lifetime.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>
    ///     The issued token. Treat the result as a credential - see the remarks on
    ///     <see cref="GitLabTokenExchangeResult" />.
    /// </returns>
    Task<GitLabTokenExchangeResult> ExchangeTokenAsync(TokenExchangeRequest request,
        CancellationToken cancellationToken = default);
}