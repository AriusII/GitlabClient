using GitLab.Client.Domain;
using GitLab.Client.Models;

namespace GitLab.Client.Abstractions;

/// <summary>
///     Wraps the registered-model half of the GitLab model registry
///     (<c>/projects/:id/ml/mlflow/api/2.0/mlflow/registered-models/*</c>) - creating, reading,
///     updating, deleting and searching the models a project has registered, plus resolving a model
///     version by alias.
///     <para>
///         These routes are GitLab's implementation of the MLflow REST API rather than a GitLab-shaped
///         API, which shows through in three places: the MLflow protocol version <c>2.0</c> is baked
///         into the path, the model name travels as a query parameter or a request body field instead
///         of as a path segment, and <see cref="SearchAsync" /> paginates with an opaque continuation
///         token rather than GitLab's usual <c>Link</c> header.
///     </para>
///     <para>
///         GitLab documents the model registry as an evolving feature and the endpoints behind it as
///         experimental. Treat this surface as less stable than the rest of the library: response
///         fields may be added, and the MLflow compatibility layer may move between GitLab releases.
///     </para>
/// </summary>
public interface IMlModelsClient
{
    /// <summary>
    ///     Registers a new model in the project. The name must be unique within the project; GitLab
    ///     answers <c>400</c> if it is already taken.
    /// </summary>
    Task<GitLabMlModel> CreateAsync(ProjectId projectId, CreateMlModelRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Fetches one registered model by name. Model names are caller-chosen free text that legally
    ///     contains dots and slashes; the name is sent as a query parameter and encoded for you, so pass
    ///     it raw.
    /// </summary>
    Task<GitLabMlModel> GetAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates a registered model's description. <see cref="UpdateMlModelRequest.Name" /> selects the
    ///     model rather than renaming it.
    /// </summary>
    Task<GitLabMlModel> UpdateAsync(ProjectId projectId, UpdateMlModelRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a registered model by name, returning the model that was removed - this endpoint
    ///     answers <c>200</c> with a body rather than <c>204</c>.
    /// </summary>
    Task<GitLabMlModel> DeleteAsync(ProjectId projectId, string name, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Searches the project's registered models. Unlike GitLab's own list endpoints this one is a
    ///     single page rather than a stream: pass
    ///     <see cref="GitLabMlModelSearchResult.NextPageToken" /> back as
    ///     <see cref="MlModelSearchOptions.PageToken" /> to fetch the next page, and stop when it comes
    ///     back empty.
    /// </summary>
    Task<GitLabMlModelSearchResult> SearchAsync(ProjectId projectId, MlModelSearchOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Fetches the latest version of a registered model. MLflow expresses this read as a <c>POST</c>
    ///     with the name in the body; the transport detail is hidden here, so pass the name directly.
    /// </summary>
    Task<GitLabMlModelVersion> GetLatestVersionAsync(ProjectId projectId, string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Resolves a model version by one of its aliases - the spec's example alias is the semantic
    ///     version <c>1.0.0</c>. Both the model name and the alias are free text and are encoded for you.
    /// </summary>
    Task<GitLabMlModelVersion> GetVersionByAliasAsync(ProjectId projectId, string name, string versionAlias,
        CancellationToken cancellationToken = default);
}