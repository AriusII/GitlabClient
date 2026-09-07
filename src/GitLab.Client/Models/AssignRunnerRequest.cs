namespace GitLab.Client.Models;

/// <summary>
///     Request body for <c>POST /projects/:id/runners</c>, which assigns an <em>existing</em> project runner
///     to another project. It does not create a runner.
/// </summary>
public sealed record AssignRunnerRequest
{
    /// <summary>The id of the runner to assign.</summary>
    public required long RunnerId { get; init; }
}