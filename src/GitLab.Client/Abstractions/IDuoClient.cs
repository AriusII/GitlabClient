using System.Text.Json;

using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps GitLab's AI surface: Duo Chat and the Duo helper endpoints (<c>/chat/completions</c>,
///     <c>/ai/llm/git_command</c>, <c>/ai/third_party_agents/direct_access</c>,
///     <c>/duo_code_review/evaluations</c>), Code Suggestions (<c>/code_suggestions</c>), GitLab Query
///     Language (<c>/glql</c>) and the Model Context Protocol server (<c>/mcp</c>).
///     <para>
///         <b>This is the youngest and least settled area of the API.</b> Access is gated: on GitLab.com
///         the Duo endpoints are documented as internal-use-only, on GitLab Self-Managed they are behind
///         the <c>access_rest_chat</c> feature flag, and
///         <see cref="GetThirdPartyAgentsDirectAccessAsync" /> is marked
///         <c>x-gitlab-lifecycle: experiment</c> in the spec. Expect
///         <see cref="Exceptions.GitLabNotFoundException" /> or
///         <see cref="Exceptions.GitLabForbiddenException" /> rather than a clean "unsupported" answer
///         where a feature is off, and expect the Duo add-on seat check to surface as a <c>403</c> too.
///     </para>
///     <para>
///         Most of these operations declare <b>no response schema at all</b> in GitLab's own OpenAPI
///         document, so this client returns their answer as a raw <see cref="JsonElement" /> rather than
///         inventing a DTO the API does not promise and that would silently start deserializing to nulls
///         the moment GitLab reshapes it. The two operations GitLab does describe -
///         <see cref="ExecuteGlqlQueryAsync" /> and, on <see cref="IMarkdownClient" />, Markdown rendering
///         - are typed. Where a response shape is known in practice it is documented per method below, as
///         prose rather than as a contract.
///     </para>
/// </summary>
public interface IDuoClient
{
    /// <summary>
    ///     Asks the AI Gateway to complete the code at the cursor
    ///     (<c>POST /code_suggestions/completions</c>).
    /// </summary>
    /// <remarks>
    ///     GitLab currently answers with an OpenAI-shaped envelope - an <c>id</c>, a <c>model</c> object,
    ///     <c>created</c>, and a <c>choices</c> array whose entries carry <c>text</c>, <c>index</c> and
    ///     <c>finish_reason</c> - but declares none of it in the spec, hence the raw
    ///     <see cref="JsonElement" />. Leave <see cref="GenerateCodeCompletionRequest.Stream" /> unset: a
    ///     streamed answer is not JSON and cannot be read through this client.
    /// </remarks>
    Task<JsonElement> GenerateCodeCompletionAsync(GenerateCodeCompletionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the caller's telemetry connection details - an instance-specific application id, an
    ///     application access token and a ClickHouse data-plane URL
    ///     (<c>POST /code_suggestions/connection_details</c>). A POST with no body.
    /// </summary>
    Task<JsonElement> GetCodeSuggestionsConnectionDetailsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves short-lived credentials that let an IDE call the AI Gateway directly instead of
    ///     proxying every keystroke through GitLab (<c>POST /code_suggestions/direct_access</c>).
    /// </summary>
    /// <param name="request">
    ///     Optional. The endpoint declares its body as not required, so passing <see langword="null" />
    ///     sends no body at all rather than an empty object.
    /// </param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <remarks>The credentials it returns are short-lived and rate limited - cache them, do not fetch one per request.</remarks>
    Task<JsonElement> GetCodeSuggestionsDirectAccessAsync(CodeSuggestionsDirectAccessRequest? request = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether Code Suggestions is available for a project, either on the project itself or
    ///     through an ancestor group's add-on (<c>POST /code_suggestions/enabled</c>).
    /// </summary>
    /// <remarks>
    ///     The answer is the status code, not a body: this completes when Code Suggestions is enabled and
    ///     throws <see cref="Exceptions.GitLabForbiddenException" /> when it is disabled. A
    ///     <see cref="Exceptions.GitLabNotFoundException" /> means the project itself was not found or is
    ///     not visible to the caller, which is a different answer from "disabled".
    /// </remarks>
    Task ValidateCodeSuggestionsEnabledAsync(CodeSuggestionsEnabledRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Turns a plain-language description into Git commands (<c>POST /ai/llm/git_command</c>).</summary>
    Task<JsonElement> GenerateGitCommandAsync(GenerateGitCommandRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves connection details, including tokens, for third-party agents
    ///     (<c>POST /ai/third_party_agents/direct_access</c>). A POST with no body.
    /// </summary>
    /// <remarks>
    ///     Marked <c>x-gitlab-lifecycle: experiment</c> in the spec - the least stable operation on this
    ///     client. It can answer <c>503</c>, which surfaces as
    ///     <see cref="Exceptions.GitLabServerException" />, when the upstream agent service is unavailable.
    /// </remarks>
    Task<JsonElement> GetThirdPartyAgentsDirectAccessAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asks GitLab Duo Chat a question, optionally about a specific issue, merge request, commit or
    ///     other resource (<c>POST /chat/completions</c>).
    /// </summary>
    /// <remarks>
    ///     Answers can be delivered asynchronously over GitLab's GraphQL subscription rather than in this
    ///     response body, which is why <see cref="DuoChatRequest.ClientSubscriptionId" /> exists. On
    ///     GitLab.com this endpoint is documented as internal-use-only; on Self-Managed it needs the
    ///     <c>access_rest_chat</c> feature flag.
    /// </remarks>
    Task<JsonElement> ChatAsync(DuoChatRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Runs Duo Code Review over a diff supplied in the request rather than over a merge request that
    ///     exists in GitLab (<c>POST /duo_code_review/evaluations</c>). Built for evaluating the reviewer
    ///     itself.
    /// </summary>
    Task<JsonElement> EvaluateCodeReviewAsync(EvaluateCodeReviewRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Executes a GitLab Query Language query - the same GLQL block that can be embedded in a
    ///     description or wiki page - and returns its rows (<c>POST /glql</c>).
    /// </summary>
    /// <remarks>
    ///     A malformed or rejected query is reported inside a <c>200</c> as
    ///     <see cref="GitLabGlqlResult.Success" /> <see langword="false" /> plus
    ///     <see cref="GitLabGlqlResult.Error" />, so check the result rather than relying on an exception.
    ///     Paging is by cursor (<see cref="GitLabGlqlPageInfo.EndCursor" /> into
    ///     <see cref="ExecuteGlqlQueryRequest.After" />), not by GitLab's usual <c>Link</c> header, so this
    ///     cannot be streamed as an <see cref="IAsyncEnumerable{T}" />.
    /// </remarks>
    Task<GitLabGlqlResult> ExecuteGlqlQueryAsync(ExecuteGlqlQueryRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the GLQL schema - the data sources and their filter, display and sort fields, the
    ///     operator, value-kind and reference-type vocabularies, the available functions, and the display
    ///     types a query can render as (<c>GET /glql/schema</c>).
    /// </summary>
    /// <remarks>
    ///     The spec declares no schema for this schema document, so it comes back as a raw
    ///     <see cref="JsonElement" />. It is the machine-readable description of what
    ///     <see cref="ExecuteGlqlQueryAsync" /> will accept, and it changes with the instance's version and
    ///     licence, so read it rather than hard-coding a vocabulary.
    /// </remarks>
    Task<JsonElement> GetGlqlSchemaAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Sends one JSON-RPC 2.0 request to GitLab's Model Context Protocol server - <c>initialize</c>,
    ///     <c>tools/list</c>, <c>tools/call</c> (<c>POST /mcp</c>).
    /// </summary>
    /// <remarks>
    ///     The response is a JSON-RPC envelope carrying either <c>result</c> or <c>error</c>, correlated to
    ///     <see cref="McpJsonRpcRequest.Id" />. A JSON-RPC-level failure is reported inside that envelope,
    ///     not as an HTTP error, so inspect the returned element rather than relying on an exception.
    /// </remarks>
    Task<JsonElement> SendMcpRequestAsync(McpJsonRpcRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads the MCP server's response listener (<c>GET /mcp</c>), the counterpart of
    ///     <see cref="SendMcpRequestAsync" /> in the Model Context Protocol's HTTP transport.
    /// </summary>
    /// <remarks>
    ///     GitLab answers <c>405 Method Not Allowed</c> where the listener is not available for the
    ///     session, which surfaces as a plain <see cref="Exceptions.GitLabApiException" /> because no
    ///     derived type is mapped to that status.
    /// </remarks>
    Task<JsonElement> ListenMcpAsync(CancellationToken cancellationToken = default);
}