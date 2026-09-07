using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Infrastructure.Serialization;
using GitLab.Client.Models;

namespace GitLab.Client.Repositories;

internal sealed class MlModelsRepository(IGitLabApiConnection connection) : IMlModelsRepository
{
    public Task<GitLabMlModel> CreateAsync(ProjectId projectId, CreateMlModelRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            RegisteredModels(projectId).Literal("create").Build(),
            request,
            GitLabJsonContext.Default.CreateMlModelRequest,
            GitLabJsonContext.Default.GitLabMlModel,
            cancellationToken);
    }

    public Task<GitLabMlModel> GetAsync(ProjectId projectId, string name,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RegisteredModels(projectId).Literal("get").Query("name", name).Build(),
            GitLabJsonContext.Default.GitLabMlModel,
            cancellationToken);
    }

    public Task<GitLabMlModel> UpdateAsync(ProjectId projectId, UpdateMlModelRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PatchAsync(
            RegisteredModels(projectId).Literal("update").Build(),
            request,
            GitLabJsonContext.Default.UpdateMlModelRequest,
            GitLabJsonContext.Default.GitLabMlModel,
            cancellationToken);
    }

    public Task<GitLabMlModel> DeleteAsync(ProjectId projectId, string name,
        CancellationToken cancellationToken = default)
    {
        // The endpoint answers 200 with the model it removed, not 204, so the body is deserialized.
        return connection.DeleteAsync(
            RegisteredModels(projectId).Literal("delete").Query("name", name).Build(),
            GitLabJsonContext.Default.GitLabMlModel,
            cancellationToken);
    }

    public Task<GitLabMlModelSearchResult> SearchAsync(ProjectId projectId, MlModelSearchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RegisteredModels(projectId).Literal("search").QueryFrom(options).Build(),
            GitLabJsonContext.Default.GitLabMlModelSearchResult,
            cancellationToken);
    }

    public Task<GitLabMlModelVersion> GetLatestVersionAsync(ProjectId projectId, string name,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            RegisteredModels(projectId).Literal("get-latest-versions").Build(),
            new MlModelLatestVersionsRequest { Name = name },
            GitLabJsonContext.Default.MlModelLatestVersionsRequest,
            GitLabJsonContext.Default.GitLabMlModelVersion,
            cancellationToken);
    }

    public Task<GitLabMlModelVersion> GetVersionByAliasAsync(ProjectId projectId, string name, string versionAlias,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            RegisteredModels(projectId).Literal("alias").Query("name", name).Query("alias", versionAlias).Build(),
            GitLabJsonContext.Default.GitLabMlModelVersion,
            cancellationToken);
    }

    /// <summary>
    ///     The prefix every registered-model route shares. GitLab mounts the MLflow protocol verbatim, so
    ///     the version lives in the path as the literal path word "2.0" - it is part of the route template,
    ///     not a caller-supplied value, hence <c>Literal</c> rather than <c>Escaped</c>. Every value a
    ///     caller supplies on this resource (a model name, an alias) is a query parameter, and
    ///     <c>GitLabRouteBuilder.Query</c> escapes those.
    /// </summary>
    private static GitLabRouteBuilder RegisteredModels(ProjectId projectId)
    {
        return GitLabRouteBuilder.Create("projects")
            .Segment(projectId)
            .Literal("ml")
            .Literal("mlflow")
            .Literal("api")
            .Literal("2.0")
            .Literal("mlflow")
            .Literal("registered-models");
    }
}