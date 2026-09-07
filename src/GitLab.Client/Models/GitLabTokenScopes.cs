namespace GitLab.Client.Models;

/// <summary>
///     The well-known values GitLab accepts in a token's <c>scopes</c> array.
///     <para>
///         These are constants rather than an enum on purpose. The pinned spec types every
///         <c>scopes</c> member as an untyped array and never enumerates the vocabulary, and GitLab
///         adds scopes in minor releases; a closed enum on a response would turn the arrival of a new
///         scope into a hard deserialization failure on an otherwise healthy listing. Use these to
///         build a request, and to compare against a response, without hard-coding string literals.
///     </para>
/// </summary>
public static class GitLabTokenScopes
{
    /// <summary>Complete read/write access to the API, including every group and project the token can reach.</summary>
    public const string Api = "api";

    /// <summary>Read-only access to the API.</summary>
    public const string ReadApi = "read_api";

    /// <summary>Read-only access to the <c>/users</c> endpoints.</summary>
    public const string ReadUser = "read_user";

    /// <summary>Read-only access to repositories, over both Git-over-HTTPS and the API.</summary>
    public const string ReadRepository = "read_repository";

    /// <summary>Read/write access to repositories, over both Git-over-HTTPS and the API.</summary>
    public const string WriteRepository = "write_repository";

    /// <summary>Read-only (pull) access to container registry images.</summary>
    public const string ReadRegistry = "read_registry";

    /// <summary>Read/write (push) access to container registry images.</summary>
    public const string WriteRegistry = "write_registry";

    /// <summary>Permission to create runners.</summary>
    public const string CreateRunner = "create_runner";

    /// <summary>Permission to manage runners.</summary>
    public const string ManageRunner = "manage_runner";

    /// <summary>Permission to make Kubernetes API calls through the GitLab agent proxy.</summary>
    public const string K8sProxy = "k8s_proxy";

    /// <summary>Permission to use GitLab Duo and other AI features.</summary>
    public const string AiFeatures = "ai_features";

    /// <summary>Permission for the token to rotate itself through the <c>self/rotate</c> endpoints.</summary>
    public const string SelfRotate = "self_rotate";

    /// <summary>Permission to act as any user. Administrators only.</summary>
    public const string Sudo = "sudo";

    /// <summary>Permission to perform API actions as an administrator when Admin Mode is enabled.</summary>
    public const string AdminMode = "admin_mode";

    /// <summary>Read-only access to the service ping payload. Administrators only.</summary>
    public const string ReadServicePing = "read_service_ping";
}