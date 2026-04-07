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

        try
        {
            var json = File.ReadAllText(ConfigPath);
            var items = JsonSerializer.Deserialize<List<PlatformConfig>>(json, JsonOptions) ?? new List<PlatformConfig>();
            return NormalizeConfigs(items);
        }
        catch (Exception)
        {
            BackupInvalidConfig();

            var defaults = GetDefaultConfigs();
            Save(defaults);
            return defaults;
        }
    }

    public void Save(IEnumerable<PlatformConfig> configs)
    {
        Directory.CreateDirectory(ConfigDirectoryPath);

        var normalized = NormalizeConfigs(configs);

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

    private static List<PlatformConfig> NormalizeConfigs(IEnumerable<PlatformConfig> configs) =>
        configs
            .Where(static config => !string.IsNullOrWhiteSpace(config.Name))
            .Select(static config => new PlatformConfig
            {
                Name = config.Name.Trim(),
                Sizes = config.NormalizedSizes.ToList()
            })
            .OrderBy(static config => config.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

    private void BackupInvalidConfig()
    {
        if (!File.Exists(ConfigPath))
        {
            return;
        }

        Directory.CreateDirectory(ConfigDirectoryPath);

        var backupPath = Path.Combine(
            ConfigDirectoryPath,
            $"config.invalid-{DateTime.Now:yyyyMMdd-HHmmss}.json");

        File.Copy(ConfigPath, backupPath, overwrite: true);
    }
}
