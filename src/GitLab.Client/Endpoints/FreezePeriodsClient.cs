using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;
using GitLab.Client.Models;
using GitLab.Client.Models.Requests;

using GitLabJsonContext = GitLab.Client.Serialization.GitLabJsonContext;

namespace GitLab.Client.Endpoints;

internal sealed class FreezePeriodsClient(IGitLabApiConnection connection) : IFreezePeriodsClient
{
    public IAsyncEnumerable<GitLabFreezePeriod> ListAsync(ProjectId projectId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetPagedAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("freeze_periods")
                .Build(),
            GitLabJsonContext.Default.GitLabFreezePeriodArray,
            cancellationToken);
    }

    public Task<GitLabFreezePeriod> GetAsync(ProjectId projectId, long freezePeriodId,
        CancellationToken cancellationToken = default)
    {
        return connection.GetAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("freeze_periods")
                .Segment(freezePeriodId)
                .Build(),
            GitLabJsonContext.Default.GitLabFreezePeriod,
            cancellationToken);
    }

    public Task<GitLabFreezePeriod> CreateAsync(ProjectId projectId, CreateFreezePeriodRequest request,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("freeze_periods")
                .Build(),
            request,
            GitLabJsonContext.Default.CreateFreezePeriodRequest,
            GitLabJsonContext.Default.GitLabFreezePeriod,
            cancellationToken);
    }

    public Task<GitLabFreezePeriod> UpdateAsync(ProjectId projectId, long freezePeriodId,
        UpdateFreezePeriodRequest request, CancellationToken cancellationToken = default)
    {
        return connection.PutAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("freeze_periods")
                .Segment(freezePeriodId)
                .Build(),
            request,
            GitLabJsonContext.Default.UpdateFreezePeriodRequest,
            GitLabJsonContext.Default.GitLabFreezePeriod,
            cancellationToken);
    }

    /// <summary>
    ///     GitLab answers this one with <c>200 OK</c> and the deleted freeze period as the body rather than
    ///     the usual <c>204</c>; <see cref="IGitLabApiConnection.DeleteAsync" /> discards it either way.
    /// </summary>
    public Task DeleteAsync(ProjectId projectId, long freezePeriodId, CancellationToken cancellationToken = default)
    {
        return connection.DeleteAsync(
            GitLabRouteBuilder.Create("projects")
                .Segment(projectId)
                .Literal("freeze_periods")
                .Segment(freezePeriodId)
                .Build(),
            cancellationToken);
    }
}