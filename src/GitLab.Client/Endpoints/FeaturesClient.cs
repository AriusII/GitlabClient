using System.Text.Json;

using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;
using GitLab.Client.Query;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class FeaturesClient(IGitLabApiConnection connection) : IFeaturesClient
{
    public IAsyncEnumerable<GitLabFeature> ListAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("features").Build(),
            GitLabJsonContext.Default.GitLabFeatureArray,
            cancellationToken);
    }

    public IAsyncEnumerable<GitLabFeatureDefinition> ListDefinitionsAsync(
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("features").Literal("definitions").Build(),
            GitLabJsonContext.Default.GitLabFeatureDefinitionArray,
            cancellationToken);
    }

    public Task<GitLabFeature> SetAsync(string name, SetFeatureRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("features").Escaped(name).Build(),
            request,
            GitLabJsonContext.Default.SetFeatureRequest,
            GitLabJsonContext.Default.GitLabFeature,
            cancellationToken);
    }

    public Task DeleteAsync(string name, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("features").Escaped(name).Build(),
            cancellationToken);
    }

    // The spec declares no response schema for either Unleash GET - the payload is Unleash's own
    // client-features document, versioned by Unleash rather than by GitLab - so it is handed back as a
    // raw JsonElement instead of a DTO shaped from guesswork.
    public Task<JsonElement> GetUnleashFeaturesAsync(ProjectId projectId, UnleashClientOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("feature_flags").Literal("unleash").Segment(projectId).QueryFrom(options)
                .Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task<JsonElement> GetUnleashClientFeaturesAsync(ProjectId projectId, UnleashClientOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("feature_flags").Literal("unleash").Segment(projectId).Literal("client")
                .Literal("features").QueryFrom(options).Build(),
            GitLabJsonContext.Default.JsonElement,
            cancellationToken);
    }

    public Task RegisterUnleashClientAsync(ProjectId projectId, UnleashClientRegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("feature_flags").Literal("unleash").Segment(projectId).Literal("client")
                .Literal("register").Build(),
            request,
            GitLabJsonContext.Default.UnleashClientRegistrationRequest,
            cancellationToken);
    }

    public Task ReportUnleashMetricsAsync(ProjectId projectId, UnleashClientRegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("feature_flags").Literal("unleash").Segment(projectId).Literal("client")
                .Literal("metrics").Build(),
            request,
            GitLabJsonContext.Default.UnleashClientRegistrationRequest,
            cancellationToken);
    }
}