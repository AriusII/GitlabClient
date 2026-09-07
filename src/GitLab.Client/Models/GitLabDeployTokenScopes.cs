namespace GitLab.Client.Models;

/// <summary>
///     The values GitLab accepts in a deploy token's <c>scopes</c> array.
///     <para>
///         Deploy tokens have their own, much narrower vocabulary than personal, project and group
///         access tokens: nothing here overlaps with <see cref="GitLabTokenScopes" /> beyond the
///         repository and container-registry names, and a deploy token can never be granted
///         <c>api</c> or <c>sudo</c>. Hence a separate constants class rather than reusing that one.
///     </para>
///     <para>
///         These are constants rather than an enum on purpose. The spec enumerates the vocabulary on
///         the <em>request</em> but types the <c>scopes</c> array on every <em>response</em> without an
///         item schema, and GitLab adds scopes in minor releases (<c>read_virtual_registry</c> and
///         <c>write_virtual_registry</c> are recent arrivals); a closed enum on a response would turn
///         the arrival of a new scope into a hard deserialization failure on an otherwise healthy
///         listing.
///     </para>
/// </summary>
public static class GitLabDeployTokenScopes
{
    /// <summary>Read-only (clone and fetch) access to the repository over Git-over-HTTPS.</summary>
    public const string ReadRepository = "read_repository";

    /// <summary>Read-only (pull) access to container registry images.</summary>
    public const string ReadRegistry = "read_registry";

    /// <summary>Read/write (push) access to container registry images.</summary>
    public const string WriteRegistry = "write_registry";

    /// <summary>Read-only (pull) access to the package registry.</summary>
    public const string ReadPackageRegistry = "read_package_registry";

    /// <summary>Read/write (publish) access to the package registry.</summary>
    public const string WritePackageRegistry = "write_package_registry";

    /// <summary>Read-only access to the virtual registry, which proxies upstream package registries.</summary>
    public const string ReadVirtualRegistry = "read_virtual_registry";

    /// <summary>Read/write access to the virtual registry.</summary>
    public const string WriteVirtualRegistry = "write_virtual_registry";
}