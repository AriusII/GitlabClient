namespace GitLab.Client.Models.Requests;

/// <summary>
///     The body of <c>POST /usage_data/increment_counter</c> and
///     <c>POST /usage_data/increment_unique_users</c> - both take only the Service Ping event name to
///     increment.
/// </summary>
public sealed record UsageDataEventRequest
{
    public required string Event { get; init; }
}