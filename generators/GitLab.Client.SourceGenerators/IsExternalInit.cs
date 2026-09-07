namespace System.Runtime.CompilerServices;

/// <summary>
///     netstandard2.0 polyfill for the marker the compiler requires to emit <c>init</c> accessors, and
///     therefore records. The generator models are <c>readonly record struct</c>s so the incremental
///     pipeline caches on value equality; without this type they would not compile (CS0518).
/// </summary>
internal static class IsExternalInit
{
}