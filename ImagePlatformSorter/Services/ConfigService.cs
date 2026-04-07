using System.Text.Encodings.Web;
using System.Text.Json;
using ImagePlatformSorter.Models;

namespace ImagePlatformSorter.Services;

public sealed class ConfigService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public string ConfigDirectoryPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Lastmach",
            "ImagePlatformSorter");

    public string ConfigPath => Path.Combine(ConfigDirectoryPath, "config.json");

    public IReadOnlyList<PlatformConfig> Load()
    {
        EnsureConfigExists();

        var json = File.ReadAllText(ConfigPath);
        var items = JsonSerializer.Deserialize<List<PlatformConfig>>(json, JsonOptions) ?? new List<PlatformConfig>();

        return items
            .Where(static item => !string.IsNullOrWhiteSpace(item.Name))
            .Select(static item => new PlatformConfig
            {
                Name = item.Name.Trim(),
                Sizes = item.NormalizedSizes.ToList()
            })
            .OrderBy(static item => item.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    public void Save(IEnumerable<PlatformConfig> configs)
    {
        Directory.CreateDirectory(ConfigDirectoryPath);

        var normalized = configs
            .Where(static config => !string.IsNullOrWhiteSpace(config.Name))
            .Select(static config => new PlatformConfig
            {
                Name = config.Name.Trim(),
                Sizes = config.NormalizedSizes.ToList()
            })
            .OrderBy(static config => config.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var json = JsonSerializer.Serialize(normalized, JsonOptions);
        File.WriteAllText(ConfigPath, json);
    }

    private void EnsureConfigExists()
    {
        if (File.Exists(ConfigPath))
        {
            return;
        }

        Save(GetDefaultConfigs());
    }

    private static IReadOnlyList<PlatformConfig> GetDefaultConfigs() =>
        new List<PlatformConfig>
        {
            new()
            {
                Name = "Яндекс",
                Sizes = new List<string> { "160x600", "240x400", "300x600" }
            },
            new()
            {
                Name = "Гибрид",
                Sizes = new List<string> { "160x600", "240x400", "300x600", "300x50" }
            },
            new()
            {
                Name = "Сберселлер",
                Sizes = new List<string> { "320x480" }
            }
        };
}

