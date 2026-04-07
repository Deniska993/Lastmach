using System.IO;
using System.Windows.Media.Imaging;

namespace ImagePlatformSorter.Services;

public sealed class ImageCompressionService
{
    public CompressionResult TrySaveWithinLimit(string sourceFilePath, string targetFilePath, int maxFileSizeKb)
    {
        var maxFileSizeBytes = maxFileSizeKb * 1024L;
        var extension = Path.GetExtension(sourceFilePath).ToLowerInvariant();

        try
        {
            using var stream = File.OpenRead(sourceFilePath);
            var decoder = BitmapDecoder.Create(
                stream,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);

            var frame = decoder.Frames.FirstOrDefault();

            if (frame is null)
            {
                return CompressionResult.Fail("Не удалось прочитать изображение.");
            }

            return extension switch
            {
                ".jpg" or ".jpeg" => TrySaveJpeg(frame, targetFilePath, maxFileSizeBytes),
                ".png" => TrySaveWithEncoder(frame, targetFilePath, maxFileSizeBytes, static () => new PngBitmapEncoder()),
                ".gif" => TrySaveWithEncoder(frame, targetFilePath, maxFileSizeBytes, static () => new GifBitmapEncoder()),
                _ => CompressionResult.Fail($"Сжатие для формата {extension} не поддерживается.")
            };
        }
        catch (Exception ex)
        {
            return CompressionResult.Fail(ex.Message);
        }
    }

    private static CompressionResult TrySaveJpeg(BitmapFrame frame, string targetFilePath, long maxFileSizeBytes)
    {
        byte[]? bestBytes = null;
        var bestQuality = 0;
        var low = 1;
        var high = 100;

        while (low <= high)
        {
            var quality = (low + high) / 2;
            var bytes = EncodeFrame(frame, () => new JpegBitmapEncoder { QualityLevel = quality });

            if (bytes.LongLength <= maxFileSizeBytes)
            {
                bestBytes = bytes;
                bestQuality = quality;
                low = quality + 1;
            }
            else
            {
                high = quality - 1;
            }
        }

        if (bestBytes is null)
        {
            return CompressionResult.Fail("Не удалось сжать JPEG до требуемого лимита.");
        }

        File.WriteAllBytes(targetFilePath, bestBytes);
        return CompressionResult.Success(bestBytes.LongLength, bestQuality);
    }

    private static CompressionResult TrySaveWithEncoder(
        BitmapFrame frame,
        string targetFilePath,
        long maxFileSizeBytes,
        Func<BitmapEncoder> encoderFactory)
    {
        var bytes = EncodeFrame(frame, encoderFactory);

        if (bytes.LongLength > maxFileSizeBytes)
        {
            return CompressionResult.Fail("Не удалось уложиться в лимит в исходном формате.");
        }

        File.WriteAllBytes(targetFilePath, bytes);
        return CompressionResult.Success(bytes.LongLength);
    }

    private static byte[] EncodeFrame(BitmapFrame frame, Func<BitmapEncoder> encoderFactory)
    {
        using var output = new MemoryStream();
        var encoder = encoderFactory();
        encoder.Frames.Add(frame);
        encoder.Save(output);
        return output.ToArray();
    }

    public sealed class CompressionResult
    {
        private CompressionResult(bool isSuccess, long outputFileSizeBytes, int? jpegQuality, string? errorMessage)
        {
            IsSuccess = isSuccess;
            OutputFileSizeBytes = outputFileSizeBytes;
            JpegQuality = jpegQuality;
            ErrorMessage = errorMessage;
        }

        public bool IsSuccess { get; }

        public long OutputFileSizeBytes { get; }

        public int? JpegQuality { get; }

        public string? ErrorMessage { get; }

        public static CompressionResult Success(long outputFileSizeBytes, int? jpegQuality = null) =>
            new(true, outputFileSizeBytes, jpegQuality, null);

        public static CompressionResult Fail(string errorMessage) =>
            new(false, 0, null, errorMessage);
    }
}
