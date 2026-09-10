using GitLab.Client.Abstractions;
using GitLab.Client.Domain;
using GitLab.Client.Infrastructure.Routing;

namespace GitLab.Client.Endpoints;

internal sealed class PackagesHelmClient(IGitLabApiConnection connection) : IPackagesHelmClient
{
    /// <summary>
    ///     The form field GitLab's Helm upload endpoint reads the chart from - "file", the
    ///     <see cref="GitLabFileUpload" /> default, is silently ignored here.
    /// </summary>
    private const string ChartFieldName = "chart";

    public Task UploadChartAsync(ProjectId projectId, string channel, GitLabFileUpload chart,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chart);

        return connection.PostFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("helm")
                .Literal("api").Escaped(channel).Literal("charts").Build(),
            chart with { FieldName = ChartFieldName },
            null,
            cancellationToken);
    }

    public Task AuthorizeChartUploadAsync(ProjectId projectId, string channel,
        CancellationToken cancellationToken = default)
    {
        return connection.PostAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("helm")
                .Literal("api").Escaped(channel).Literal("charts").Literal("authorize").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadChartAsync(ProjectId projectId, string channel, string fileName,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("helm")
                .Escaped(channel).Literal("charts")
                // The ".tgz" suffix is part of GitLab's route template, not caller-supplied text, but there
                // is no builder method to append a literal after an already-escaped segment - so the whole
                // "name.tgz" is escaped as one unit. Uri.EscapeDataString leaves '.' untouched, so this
                // produces exactly the same bytes as escaping the name alone and appending ".tgz" literally.
                .Escaped($"{fileName}.tgz").Build(),
            cancellationToken);
    }

    public Task<GitLabFileResponse> DownloadChartIndexAsync(ProjectId projectId, string channel,
        CancellationToken cancellationToken = default)
    {
        return connection.GetFileAsync(
            GitLabRouteBuilder.Create("projects").Segment(projectId).Literal("packages").Literal("helm")
                .Escaped(channel).Literal("index.yaml").Build(),
            cancellationToken);
    }
}