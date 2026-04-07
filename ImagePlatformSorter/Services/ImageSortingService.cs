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

    private readonly ImageCompressionService _compressionService = new();

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
        var compressedFiles = 0;
        var compressionErrorFiles = 0;
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

                if (platform.MaxFileSizeKb is > 0)
                {
                    var limitResult = TryCopyOrCompressWithinLimit(filePath, targetFilePath, fileName, platform, platformFolderName, logs);

                    switch (limitResult)
                    {
                        case LimitedCopyResult.CopiedOriginal:
                            copiedFiles++;
                            break;
                        case LimitedCopyResult.Compressed:
                            copiedFiles++;
                            compressedFiles++;
                            break;
                        default:
                            skippedFiles++;
                            compressionErrorFiles++;
                            break;
                    }

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
            CompressedFiles = compressedFiles,
            CompressionErrorFiles = compressionErrorFiles,
            SkippedFiles = skippedFiles,
            UnmatchedFiles = unmatchedFiles,
            OutputFolder = outputRoot,
            LogLines = logs
        };
    }

    private LimitedCopyResult TryCopyOrCompressWithinLimit(
        string sourceFilePath,
        string targetFilePath,
        string fileName,
        PlatformConfig platform,
        string platformFolderName,
        List<string> logs)
    {
        var sourceFileSizeBytes = new FileInfo(sourceFilePath).Length;
        var maxFileSizeBytes = platform.MaxFileSizeKb!.Value * 1024L;

        if (sourceFileSizeBytes <= maxFileSizeBytes)
        {
            File.Copy(sourceFilePath, targetFilePath, overwrite: true);
            logs.Add($"РЎРєРѕРїРёСЂРѕРІР°РЅРѕ: {fileName} -> {platformFolderName} (Р»РёРјРёС‚ {platform.MaxFileSizeKb} KB, СЃР¶Р°С‚РёРµ РЅРµ С‚СЂРµР±СѓРµС‚СЃСЏ)");
            return LimitedCopyResult.CopiedOriginal;
        }

        var compressionResult = _compressionService.TrySaveWithinLimit(
            sourceFilePath,
            targetFilePath,
            platform.MaxFileSizeKb.Value);

        if (!compressionResult.IsSuccess)
        {
            logs.Add($"РћС€РёР±РєР° Р»РёРјРёС‚Р°: {fileName} -> {platformFolderName}. {compressionResult.ErrorMessage}");
            return LimitedCopyResult.Failed;
        }

        var qualityPart = compressionResult.JpegQuality is > 0
            ? $", quality {compressionResult.JpegQuality}"
            : string.Empty;

        logs.Add($"РЎР¶Р°С‚Рѕ: {fileName} -> {platformFolderName} ({Math.Ceiling(compressionResult.OutputFileSizeBytes / 1024d):0} KB / Р»РёРјРёС‚ {platform.MaxFileSizeKb} KB{qualityPart})");
        return LimitedCopyResult.Compressed;
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

    private enum LimitedCopyResult
    {
        CopiedOriginal,
        Compressed,
        Failed
    }
}
