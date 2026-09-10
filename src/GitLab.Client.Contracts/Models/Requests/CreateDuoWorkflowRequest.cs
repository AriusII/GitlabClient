namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body of <c>POST /ai/duo_workflows/workflows</c> - creates and starts a flow.
///     <para>
///         Every member is optional. GitLab validates the combination server-side (which fields are
///         needed depends on <see cref="WorkflowDefinition" /> and on whether the flow is project- or
///         namespace-scoped), so this type deliberately does not guess at a required set the spec does
///         not declare.
///     </para>
/// </summary>
public sealed record CreateDuoWorkflowRequest
{
    /// <summary>The ID or path of the project the flow runs in.</summary>
    public string? ProjectId { get; init; }

    /// <summary>The ID or path of the namespace the flow runs in.</summary>
    public string? NamespaceId { get; init; }

    /// <summary>The AI Catalog item consumer that configures which catalog item to execute.</summary>
    public long? AiCatalogItemConsumerId { get; init; }

    /// <summary>Start the flow in a CI pipeline. GitLab marks this parameter itself experimental.</summary>
    public bool? StartWorkflow { get; init; }

    /// <summary>What the flow is being asked to do.</summary>
    public string? Goal { get; init; }

    /// <summary>The flow type, by capability - for example <c>software_developer</c>.</summary>
    public string? WorkflowDefinition { get; init; }

    /// <summary>
    ///     Whether the agent may stop and ask the user questions. GitLab defaults it to
    ///     <see langword="true" />; set it to <see langword="false" /> to make the flow run straight through.
    /// </summary>
    public bool? AllowAgentToRequestUser { get; init; }

    /// <summary>Container image to run the flow in when it is started as a CI pipeline.</summary>
    public string? Image { get; init; }

    /// <summary>Source branch for the CI pipeline. GitLab uses the default branch when omitted.</summary>
    public string? SourceBranch { get; init; }

    /// <summary>Where the flow is being started from.</summary>
    public GitLabDuoWorkflowEnvironment? Environment { get; init; }

    /// <summary>The AI Catalog item version that sourced the flow config.</summary>
    public long? AiCatalogItemVersionId { get; init; }

    /// <summary>Extra context handed to the flow, each entry carrying a category and its content.</summary>
    public IReadOnlyList<DuoWorkflowAdditionalContext>? AdditionalContext { get; init; }

    /// <summary>IID of the issue the flow is associated with.</summary>
    public long? IssueId { get; init; }

    /// <summary>IID of the merge request the flow is associated with.</summary>
    public long? MergeRequestId { get; init; }

    /// <summary>The actions the agent is allowed to perform.</summary>
    public IReadOnlyList<int>? AgentPrivileges { get; init; }

    /// <summary>The actions the agent may perform without asking for approval.</summary>
    public IReadOnlyList<int>? PreApprovedAgentPrivileges { get; init; }

    /// <summary>What triggered this session.</summary>
    public GitLabDuoWorkflowSource? Source { get; init; }

    /// <summary>
    ///     <see cref="GitLabDuoWorkflowFlowCallbackHook.Id" /> of a registered callback endpoint. When set,
    ///     GitLab POSTs flow lifecycle events there instead of leaving the client to poll. The hook must
    ///     belong to the project or namespace the flow runs in, or to one of its ancestor groups.
    /// </summary>
    public long? CallbackHookId { get; init; }

    /// <summary>
    ///     An opaque string echoed back in every callback payload, for correlating callbacks with the
    ///     request that triggered them. At most 255 characters.
    /// </summary>
    public string? ClientReference { get; init; }
}