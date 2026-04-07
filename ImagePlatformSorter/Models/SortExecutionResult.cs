namespace ImagePlatformSorter.Models;

public sealed class SortExecutionResult
{
    public int ProcessedImageFiles { get; init; }

    public int CopiedFiles { get; init; }

    public int SkippedFiles { get; init; }

    public int UnmatchedFiles { get; init; }

    public string OutputFolder { get; init; } = string.Empty;

    public List<string> LogLines { get; init; } = new();

    public string ToMultilineText()
    {
        var lines = new List<string>
        {
            $"Готово. Выходная папка: {OutputFolder}",
            $"Обработано изображений: {ProcessedImageFiles}",
            $"Скопировано файлов: {CopiedFiles}",
            $"Пропущено файлов: {SkippedFiles}",
            $"Без совпадений по площадкам: {UnmatchedFiles}"
        };

        if (LogLines.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("Подробности:");
            lines.AddRange(LogLines);
        }

        return string.Join(Environment.NewLine, lines);
    }
}

