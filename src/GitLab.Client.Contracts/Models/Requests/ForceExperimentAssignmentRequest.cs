namespace GitLab.Client.Models.Requests;

/// <summary>Body of <c>POST /experiments/:experiment_name/assignments</c>.</summary>
public sealed record ForceExperimentAssignmentRequest
{
    /// <summary>The variant to force - "control", "candidate", or whatever the experiment defines.</summary>
    public required string Variant { get; init; }

    /// <summary>
    ///     Context parameters identifying who/what the assignment applies to - "user", "namespace", "project"
    ///     mapped to the relevant GitLab ID, as text. Defaults to the current user when omitted. The spec
    ///     types this as an untyped object with no declared keys, so it is carried as a plain string
    ///     dictionary rather than a shape the spec does not promise.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Context { get; init; }
}