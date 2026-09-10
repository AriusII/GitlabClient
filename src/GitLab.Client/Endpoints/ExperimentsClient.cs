using GitLab.Client.Abstractions;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class ExperimentsClient(IGitLabApiConnection connection) : IExperimentsClient
{
    public IAsyncEnumerable<GitLabExperiment> ListAsync(CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("experiments").Build(),
            GitLabJsonContext.Default.GitLabExperimentArray,
            cancellationToken);
    }

    public Task<GitLabExperimentAssignment> GetAssignmentAsync(string experimentName,
        IReadOnlyDictionary<string, string>? context = null, CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            AssignmentsRoute(experimentName, context),
            GitLabJsonContext.Default.GitLabExperimentAssignment,
            cancellationToken);
    }

    public Task<GitLabExperimentAssignment> ForceAssignmentAsync(string experimentName,
        ForceExperimentAssignmentRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("experiments").Escaped(experimentName).Literal("assignments").Build(),
            request,
            GitLabJsonContext.Default.ForceExperimentAssignmentRequest,
            GitLabJsonContext.Default.GitLabExperimentAssignment,
            cancellationToken);
    }

    public Task ClearAssignmentAsync(string experimentName, IReadOnlyDictionary<string, string>? context = null,
        CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(AssignmentsRoute(experimentName, context), cancellationToken);
    }

    public Task DeleteCacheAsync(string experimentName, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("experiments").Escaped(experimentName).Literal("cache").Build(),
            cancellationToken);
    }

    /// <summary>
    ///     Builds the shared <c>/experiments/:experiment_name/assignments</c> route for the GET and DELETE
    ///     operations, which take an identical shape: a free-text context bag with no fixed key set. GitLab
    ///     declares it as <c>context[user]=...&amp;context[project]=...</c>, so each entry is written as its
    ///     own <c>context[key]</c> parameter rather than through the <c>[GitLabQuery]</c> generator, which
    ///     has no mapping for an open-ended object.
    /// </summary>
    private static Uri AssignmentsRoute(string experimentName, IReadOnlyDictionary<string, string>? context)
    {
        GitLabRouteBuilder builder =
            GitLabRouteBuilder.Create("experiments").Escaped(experimentName).Literal("assignments");

        if (context is not null)
        {
            foreach ((string key, string value) in context)
            {
                // The key is caller-supplied free text too - GitLabRouteBuilder.Query only escapes the
                // value, so an unescaped key could otherwise break the query string or smuggle in an
                // extra parameter.
                builder.Query($"context[{Uri.EscapeDataString(key)}]", value);
            }
        }

        return builder.Build();
    }
}