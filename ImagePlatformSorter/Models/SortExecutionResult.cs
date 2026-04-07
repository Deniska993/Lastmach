namespace ImagePlatformSorter.Models;

public sealed class SortExecutionResult
{
    public int ProcessedImageFiles { get; init; }

    public int CopiedFiles { get; init; }

    public int CompressedFiles { get; init; }

    public int CompressionErrorFiles { get; init; }

    public int SkippedFiles { get; init; }

    public int UnmatchedFiles { get; init; }

    public string OutputFolder { get; init; } = string.Empty;

    public List<string> LogLines { get; init; } = new();

    public string ToMultilineText()
    {
        var lines = new List<string>
        {
            $"Р“РѕС‚РѕРІРѕ. Р’С‹С…РѕРґРЅР°СЏ РїР°РїРєР°: {OutputFolder}",
            $"РћР±СЂР°Р±РѕС‚Р°РЅРѕ РёР·РѕР±СЂР°Р¶РµРЅРёР№: {ProcessedImageFiles}",
            $"РЎРѕР·РґР°РЅРѕ С„Р°Р№Р»РѕРІ: {CopiedFiles}",
            $"РЎР¶Р°С‚Рѕ С„Р°Р№Р»РѕРІ: {CompressedFiles}",
            $"РћС€РёР±РѕРє РїРѕ Р»РёРјРёС‚Сѓ РІРµСЃР°: {CompressionErrorFiles}",
            $"РџСЂРѕРїСѓС‰РµРЅРѕ С„Р°Р№Р»РѕРІ: {SkippedFiles}",
            $"Р‘РµР· СЃРѕРІРїР°РґРµРЅРёР№ РїРѕ РїР»РѕС‰Р°РґРєР°Рј: {UnmatchedFiles}"
        };

        if (LogLines.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("РџРѕРґСЂРѕР±РЅРѕСЃС‚Рё:");
            lines.AddRange(LogLines);
        }

        return string.Join(Environment.NewLine, lines);
    }
}
