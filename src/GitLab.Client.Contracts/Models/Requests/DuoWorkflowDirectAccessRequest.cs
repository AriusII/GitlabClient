namespace GitLab.Client.Models.Requests;

/// <summary>
///     Body of <c>POST /ai/duo_workflows/direct_access</c>. Every member is optional, so an empty
///     instance is a valid request.
/// </summary>
public sealed record DuoWorkflowDirectAccessRequest
{
    /// <summary>The flow type, by capability - for example <c>software_developer</c>.</summary>
    public string? WorkflowDefinition { get; init; }

    /// <summary>The ID or path of the root namespace.</summary>
    public string? RootNamespaceId { get; init; }

    /// <summary>The ID or path of the project. Sent in the body, so pass it unencoded.</summary>
    public string? ProjectId { get; init; }
}