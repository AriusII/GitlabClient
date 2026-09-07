namespace GitLab.Client.SourceGenerators;

/// <summary>
///     Names attached to the incremental pipeline nodes with <c>WithTrackingName</c>. Nothing in a normal
///     build tells you a generator stopped caching, so the incrementality tests assert on these step
///     names directly; without them there is no way to pin the value-equatable models in place.
/// </summary>
public static class TrackingNames
{
    public const string LayerModels = "ClientLayerModels";

    public const string LayerCollisions = "ClientLayerCollisions";

    public const string WiringModels = "ResourceWiringModels";

    public const string SortedWiringModels = "SortedResourceWiringModels";
}