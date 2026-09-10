namespace GitLab.Client.Models.Requests;

/// <summary>
///     The optional body of <c>PUT /admin/data_management/:model_name/checksum</c>. Leaving both members
///     unset recalculates checksums for every record of the model.
/// </summary>
public sealed record RecalculateModelChecksumsRequest
{
    /// <summary>
    ///     Restricts the recalculation to these record identifiers. GitLab accepts either numbers or
    ///     strings here depending on the model, so every element is sent as text; Grape coerces it back to
    ///     the underlying column type.
    /// </summary>
    public IReadOnlyList<string>? Identifiers { get; init; }

    /// <summary>
    ///     Restricts the recalculation to records currently in this checksum state - <c>started</c>,
    ///     <c>succeeded</c>, <c>failed</c> or <c>disabled</c>. Left as free text rather than an enum: the
    ///     read-side filter on <see cref="AdminModelListOptions.ChecksumState" /> additionally allows
    ///     <c>pending</c> for the same wire name, so one shared enum would either reject a valid write
    ///     value or accept an invalid one.
    /// </summary>
    public string? ChecksumState { get; init; }
}