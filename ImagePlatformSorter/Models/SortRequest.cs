namespace ImagePlatformSorter.Models;

public sealed class SortRequest
{
    public string InputFolder { get; init; } = string.Empty;

    public string? OutputFolder { get; init; }

    public IReadOnlyList<PlatformConfig> Platforms { get; init; } = Array.Empty<PlatformConfig>();

    public bool OverwriteExisting { get; init; }
}

