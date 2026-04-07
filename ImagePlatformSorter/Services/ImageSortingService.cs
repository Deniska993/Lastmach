using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using ImagePlatformSorter.Models;

namespace ImagePlatformSorter.Services;

public sealed class ImageSortingService
{
    private static readonly Regex SizePattern = new(@"(?<!\d)(\d{2,5}\s*[xС…XРҐ]\s*\d{2,5})(?!\d)", RegexOptions.Compiled);

    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".bmp",
        ".gif"
    };

    public SortExecutionResult Sort(SortRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.InputFolder) || !Directory.Exists(request.InputFolder))
        {
            throw new DirectoryNotFoundException("РќРµ РЅР°Р№РґРµРЅР° РІС…РѕРґРЅР°СЏ РїР°РїРєР°.");
        }

        if (request.Platforms.Count == 0)
        {
            throw new InvalidOperationException("РќРµ РІС‹Р±СЂР°РЅР° РЅРё РѕРґРЅР° РїР»РѕС‰Р°РґРєР°.");
        }

        var outputRoot = string.IsNullOrWhiteSpace(request.OutputFolder)
            ? Path.Combine(request.InputFolder, "Sorted")
            : request.OutputFolder.Trim();

        Directory.CreateDirectory(outputRoot);

        var logs = new List<string>();
        var copiedFiles = 0;
        var skippedFiles = 0;
        var unmatchedFiles = 0;
        var processedImageFiles = 0;

        foreach (var filePath in Directory.EnumerateFiles(request.InputFolder).OrderBy(Path.GetFileName, StringComparer.CurrentCultureIgnoreCase))
        {
            if (!SupportedExtensions.Contains(Path.GetExtension(filePath)))
            {
                continue;
            }

            processedImageFiles++;

            var fileName = Path.GetFileName(filePath);
            var size = TryGetSize(filePath);

            if (string.IsNullOrWhiteSpace(size))
            {
                skippedFiles++;
                logs.Add($"РџСЂРѕРїСѓСЃРє: РЅРµ СѓРґР°Р»РѕСЃСЊ РѕРїСЂРµРґРµР»РёС‚СЊ СЂР°Р·РјРµСЂ С„Р°Р№Р»Р° {fileName}.");
                continue;
            }

            var matchedPlatforms = request.Platforms
                .Where(platform => platform.MatchesSize(size))
                .ToList();

            if (matchedPlatforms.Count == 0)
            {
                unmatchedFiles++;
                logs.Add($"Р‘РµР· СЃРѕРІРїР°РґРµРЅРёСЏ: {fileName} ({size}) РЅРµ РїРѕРґС…РѕРґРёС‚ РЅРё РѕРґРЅРѕР№ РІС‹Р±СЂР°РЅРЅРѕР№ РїР»РѕС‰Р°РґРєРµ.");
                continue;
            }

            foreach (var platform in matchedPlatforms)
            {
                var platformFolderName = SanitizeFolderName(platform.Name);
                var targetDirectory = Path.Combine(outputRoot, platformFolderName);
                var targetFilePath = Path.Combine(targetDirectory, fileName);

                Directory.CreateDirectory(targetDirectory);

                if (File.Exists(targetFilePath) && !request.OverwriteExisting)
                {
                    skippedFiles++;
                    logs.Add($"РџСЂРѕРїСѓСЃРє: С„Р°Р№Р» {fileName} СѓР¶Рµ СЃСѓС‰РµСЃС‚РІСѓРµС‚ РІ РїР°РїРєРµ {platformFolderName}.");
                    continue;
                }

                File.Copy(filePath, targetFilePath, request.OverwriteExisting);
                copiedFiles++;
                logs.Add($"РЎРєРѕРїРёСЂРѕРІР°РЅРѕ: {fileName} -> {platformFolderName}");
            }
        }

        return new SortExecutionResult
        {
            ProcessedImageFiles = processedImageFiles,
            CopiedFiles = copiedFiles,
            SkippedFiles = skippedFiles,
            UnmatchedFiles = unmatchedFiles,
            OutputFolder = outputRoot,
            LogLines = logs
        };
    }

    private static string? TryGetSize(string filePath)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        var match = SizePattern.Match(fileNameWithoutExtension);

        if (match.Success)
        {
            return PlatformConfig.NormalizeSize(match.Groups[1].Value);
        }

        try
        {
            using var stream = File.OpenRead(filePath);
            var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
            var frame = decoder.Frames.FirstOrDefault();

            if (frame is null)
            {
                return null;
            }

            return $"{frame.PixelWidth}x{frame.PixelHeight}";
        }
        catch
        {
            return null;
        }
    }

    private static string SanitizeFolderName(string folderName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var safeName = new string(folderName.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray()).Trim();

        if (string.IsNullOrWhiteSpace(safeName))
        {
            return "РџР»РѕС‰Р°РґРєР°";
        }

        return safeName.TrimEnd('.', ' ');
    }
}
