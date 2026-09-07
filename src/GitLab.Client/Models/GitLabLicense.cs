using System.Text.Json;

namespace GitLab.Client.Models;

/// <summary>
///     An instance licence, as returned by the GitLab Licence API (<c>/license</c>, <c>/licenses</c>,
///     <c>/license/:id</c>) - the key that activates GitLab Enterprise Edition, seen from the
///     administrator's side.
///     <para>
///         The listing endpoint (<c>GET /licenses</c>) returns <c>APIEntitiesGitlabLicense</c> and the
///         single-licence endpoints return <c>APIEntitiesGitlabLicenseWithActiveUsers</c>; the two differ
///         only by <see cref="ActiveUsers" />, which is why that member is nullable rather than a second
///         record.
///     </para>
///     <para>
///         The licence key itself is never echoed back by GitLab, and this record deliberately has no
///         member for it - see <see cref="CreateLicenseRequest" />.
///     </para>
/// </summary>
public sealed record GitLabLicense
{
    public required long Id { get; init; }

    /// <summary>The subscription tier the licence unlocks - <c>premium</c>, <c>ultimate</c>, <c>starter</c>.</summary>
    public string? Plan { get; init; }

    /// <summary>When the licence was uploaded to the instance, not when it was issued.</summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>The first day the licence is valid. GitLab types this one as a plain date.</summary>
    public DateOnly? StartsAt { get; init; }

    /// <summary>The last day the licence is valid. GitLab types this one as a plain date.</summary>
    public DateOnly? ExpiresAt { get; init; }

    /// <summary>The highest billable user count seen during the licence term.</summary>
    public int? HistoricalMax { get; init; }

    /// <summary>The number of users the licence was purchased for.</summary>
    public int? MaximumUserCount { get; init; }

    /// <summary>
    ///     Who the licence was issued to. GitLab declares this as a bare object and returns PascalCase keys
    ///     inside it (<c>Name</c>, <c>Email</c>, <c>Company</c>), so it is captured as a raw
    ///     <see cref="JsonElement" /> rather than forced into a shape the spec does not promise.
    /// </summary>
    public JsonElement? Licensee { get; init; }

    /// <summary>
    ///     The add-on seat counts, keyed by add-on name. Declared as a bare object by GitLab, and kept as a
    ///     raw <see cref="JsonElement" /> for the same reason as <see cref="Licensee" />.
    /// </summary>
    public JsonElement? AddOns { get; init; }

    public bool? Expired { get; init; }

    /// <summary>
    ///     The difference between the billable user count and the licensed user count. GitLab computes it
    ///     from <see cref="HistoricalMax" /> on an expired licence and from the current count otherwise.
    /// </summary>
    public int? Overage { get; init; }

    public int? UserLimit { get; init; }

    /// <summary>
    ///     The current billable user count. Returned by the single-licence endpoints only - the
    ///     <c>GET /licenses</c> listing omits it.
    /// </summary>
    public int? ActiveUsers { get; init; }
}