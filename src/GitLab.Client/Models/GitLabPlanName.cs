using System.Text.Json.Serialization;

namespace GitLab.Client.Models;

/// <summary>The billing plans a GitLab instance's <c>plan_limits</c> can be read or changed for.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GitLabPlanName>))]
public enum GitLabPlanName
{
    [JsonStringEnumMemberName("default")] Default,

    [JsonStringEnumMemberName("free")] Free,

    [JsonStringEnumMemberName("bronze")] Bronze,

    [JsonStringEnumMemberName("silver")] Silver,

    [JsonStringEnumMemberName("premium")] Premium,

    [JsonStringEnumMemberName("gold")] Gold,

    [JsonStringEnumMemberName("ultimate")] Ultimate,

    [JsonStringEnumMemberName("ultimate_trial")]
    UltimateTrial,

    [JsonStringEnumMemberName("ultimate_trial_paid_customer")]
    UltimateTrialPaidCustomer,

    [JsonStringEnumMemberName("premium_trial")]
    PremiumTrial,

    [JsonStringEnumMemberName("opensource")]
    Opensource
}