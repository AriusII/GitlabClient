namespace GitLab.Client.Composition.WorkItems;

/// <summary>The outcome of one explicit, safe cross-source work-item correlation.</summary>
public enum GitLabWorkItemCompositionCorrelationStatus
{
    /// <summary>At least one side of the comparison was not supplied or did not expose the needed field.</summary>
    Unavailable,

    /// <summary>The comparable values agree.</summary>
    Matches,

    /// <summary>The comparable values disagree.</summary>
    Mismatch,

    /// <summary>A supplied identifier could not be represented in the comparison's documented domain.</summary>
    Invalid
}