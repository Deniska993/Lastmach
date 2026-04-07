using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ImagePlatformSorter.Models;

public sealed class PlatformConfig
{
    private static readonly Regex SizeRegex = new(@"^\s*(\d+)\s*[xС…XРҐ]\s*(\d+)\s*$", RegexOptions.Compiled);
    private static readonly Regex SizeExtractorRegex = new(@"(\d+\s*[xС…XРҐ]\s*\d+)", RegexOptions.Compiled);

    public string Name { get; set; } = string.Empty;

    public List<string> Sizes { get; set; } = new();

    public int? MaxFileSizeKb { get; set; }

    [JsonIgnore]
    public IReadOnlyCollection<string> NormalizedSizes =>
        Sizes
            .Select(NormalizeSize)
            .Where(static size => !string.IsNullOrWhiteSpace(size))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    [JsonIgnore]
    public string SizesDisplay => string.Join(", ", NormalizedSizes);

    [JsonIgnore]
    public string MaxFileSizeDisplay => MaxFileSizeKb is > 0 ? $"{MaxFileSizeKb} KB" : "Без лимита";

    public bool MatchesSize(string size) =>
        NormalizedSizes.Contains(NormalizeSize(size), StringComparer.OrdinalIgnoreCase);

    public PlatformConfig Clone() =>
        new()
        {
            Name = Name,
            Sizes = Sizes.ToList(),
            MaxFileSizeKb = MaxFileSizeKb
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
            : value.Trim().Replace('С…', 'x').Replace('РҐ', 'x').Replace(" ", string.Empty, StringComparison.Ordinal);
    }

    public static IReadOnlyList<string> ExtractSizes(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Array.Empty<string>();
        }

        var matches = SizeExtractorRegex.Matches(input);

        if (matches.Count == 0)
        {
            var fallback = NormalizeSize(input);
            return string.IsNullOrWhiteSpace(fallback)
                ? Array.Empty<string>()
                : new[] { fallback };
        }

        return matches
            .Select(static match => NormalizeSize(match.Value))
            .Where(static size => !string.IsNullOrWhiteSpace(size))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
