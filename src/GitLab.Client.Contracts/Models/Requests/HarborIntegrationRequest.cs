namespace GitLab.Client.Models.Requests;

/// <summary>
///     Typed settings for the <c>harbor</c> integration - see <see cref="GitLabIntegrationSlug.Harbor" />.
///     Links a project to a Harbor container registry instance.
/// </summary>
public sealed record HarborIntegrationRequest
{
    /// <summary>
    ///     The base URL to the Harbor instance linked to the GitLab project (for example,
    ///     <c>https://demo.goharbor.io</c>).
    /// </summary>
    public required Uri Url { get; init; }

    /// <summary>The name of the project in the Harbor instance (for example, <c>testproject</c>).</summary>
    public required string ProjectName { get; init; }

    /// <summary>The username created in the Harbor interface.</summary>
    public required string Username { get; init; }

    /// <summary>The password of the user.</summary>
    public required string Password { get; init; }

    /// <summary>Indicates whether to inherit the default settings. Defaults to <see langword="false" />.</summary>
    public bool? UseInheritedSettings { get; init; }
}