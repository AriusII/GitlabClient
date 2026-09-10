namespace GitLab.Client.Models;

/// <summary>The JSON shape carried by a <see cref="GitLabPipelineInputValue" />.</summary>
public enum GitLabPipelineInputValueKind
{
    /// <summary>An explicit JSON <see langword="null" />.</summary>
    Null,

    /// <summary>A JSON string.</summary>
    Text,

    /// <summary>A JSON number.</summary>
    Number,

    /// <summary>A JSON Boolean.</summary>
    Flag,

    /// <summary>A JSON array whose entries use the same input-value union.</summary>
    Sequence
}