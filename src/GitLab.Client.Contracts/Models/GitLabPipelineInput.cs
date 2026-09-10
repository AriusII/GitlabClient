namespace GitLab.Client.Models;

/// <summary>
///     A named CI/CD input on a pipeline schedule. Create and update schedule requests carry these as an
///     array, and schedule responses expose the same named value shape.
/// </summary>
/// <remarks>
///     <see cref="Destroy" /> is meaningful only while updating a schedule. Leaving it unset omits it from
///     the JSON payload, so the input is created or updated instead.
/// </remarks>
public sealed record GitLabPipelineInput
{
    /// <summary>The input name declared by the CI/CD configuration.</summary>
    public required string Name { get; init; }

    /// <summary>Deletes this input when sent to the update-schedule endpoint.</summary>
    public bool? Destroy { get; init; }

    /// <summary>The typed input value.</summary>
    public required GitLabPipelineInputValue Value { get; init; }
}