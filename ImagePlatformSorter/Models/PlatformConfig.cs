using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ImagePlatformSorter.Models;

public sealed class PlatformConfig
{
    private static readonly Regex SizeRegex = new(@"^\s*(\d+)\s*[xхXХ]\s*(\d+)\s*$", RegexOptions.Compiled);

    public string Name { get; set; } = string.Empty;

    public List<string> Sizes { get; set; } = new();

    [JsonIgnore]
    public IReadOnlyCollection<string> NormalizedSizes =>
        Sizes
            .Select(NormalizeSize)
            .Where(static size => !string.IsNullOrWhiteSpace(size))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    [JsonIgnore]
    public string SizesDisplay => string.Join(", ", NormalizedSizes);

    public bool MatchesSize(string size) =>
        NormalizedSizes.Contains(NormalizeSize(size), StringComparer.OrdinalIgnoreCase);

    public PlatformConfig Clone() =>
        new()
        {
            Name = Name,
            Sizes = Sizes.ToList()
        };

    public static string NormalizeSize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var match = SizeRegex.Match(value);
        return match.Success
            ? $"{match.Groups[1].Value}x{match.Groups[2].Value}"
            : value.Trim().Replace('х', 'x').Replace('Х', 'x').Replace(" ", string.Empty, StringComparison.Ordinal);
    }
}

