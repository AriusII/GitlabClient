namespace GitLab.Client.Models.Requests;

/// <summary>A variable supplied while creating a pipeline through <c>POST /projects/:id/pipeline</c>.</summary>
public sealed record GitLabPipelineVariableRequest
{
    /// <summary>The CI/CD variable name.</summary>
    public string? Key { get; init; }

    /// <summary>The variable's value.</summary>
    public string? Value { get; init; }

    /// <summary>Whether GitLab supplies the value as an environment variable or as a temporary file.</summary>
    public GitLabPipelineVariableType? VariableType { get; init; }
}