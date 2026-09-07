using System.Diagnostics.CodeAnalysis;

namespace GitLab.Client.Models;

/// <summary>
///     What <c>model-versions/get-download-uri</c> answers with: where the model version's artifacts can
///     be fetched from.
/// </summary>
public sealed record GitLabMlflowArtifactUri
{
    /// <summary>
    ///     The download location, in MLflow's own <c>mlflow-artifacts:&lt;version&gt;</c> form. A
    ///     <see cref="string" /> rather than a <see cref="Uri" /> because that form is an opaque MLflow
    ///     reference a client resolves against its own tracking server, not a fetchable absolute URL.
    /// </summary>
    [SuppressMessage("Design", "CA1056",
        Justification =
            "GitLab answers with MLflow's opaque 'mlflow-artifacts:<version>' reference, which the MLflow "
            + "client resolves rather than dereferences as a URL.")]
    public string? ArtifactUri { get; init; }
}