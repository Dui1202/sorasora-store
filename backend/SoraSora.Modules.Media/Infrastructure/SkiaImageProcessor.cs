using SkiaSharp;
using SoraSora.Modules.Media.Contracts;
using SoraSora.Modules.Media.Domain;
using SoraSora.Modules.Media.Exceptions;

namespace SoraSora.Modules.Media.Infrastructure;

/// <summary>
/// Treats uploaded product images as untrusted input and converts them into a
/// server-controlled JPEG, PNG, or WebP payload.
/// </summary>
/// <remarks>
/// The submitted filename and content type are intentionally ignored. SkiaSharp detects the
/// encoded format from the file bytes, fully decodes the pixels, applies EXIF orientation, and
/// re-encodes the image. The resulting format is then used by the application to choose the
/// storage content type and file extension.
/// </remarks>
public sealed class SkiaImageProcessor : IImageProcessor
{
    private const int LossyImageQuality = 90;

    /// <summary>
    /// Validates, decodes, normalizes, and re-encodes one uploaded image.
    /// </summary>
    /// <param name="source">The untrusted encoded image stream.</param>
    /// <param name="sourceLength">
    /// The uploaded stream length, used to reject oversized input before native decoding.
    /// </param>
    /// <param name="cancellationToken">Cancels processing between native operations.</param>
    /// <returns>
    /// Canonical image bytes together with the server-detected format and final dimensions.
    /// </returns>
    /// <exception cref="MediaProcessingException">
    /// The input is empty, malformed, unsupported, animated, too large, or cannot be encoded.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// <paramref name="cancellationToken"/> has been cancelled.
    /// </exception>
    public ProcessedImage Process(
        Stream source,
        long sourceLength,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(source);
        cancellationToken.ThrowIfCancellationRequested();

        if (sourceLength <= 0 || sourceLength > MediaUploadLimits.MaxImageBytes)
        {
            throw new MediaProcessingException("Image file must be between 1 byte and 5 MB.");
        }

        // Read only format and dimensions first. Pixel memory is not allocated until the
        // dimension and frame-count limits have rejected decompression bombs.
        using var codec = CreateCodec(source);
        var format = GetSupportedFormat(codec.EncodedFormat);
        ValidateImageMetadata(codec);

        var decodeInfo = new SKImageInfo(
            codec.Info.Width,
            codec.Info.Height,
            SKColorType.Rgba8888,
            SKAlphaType.Premul
        );
        using var decodedBitmap = new SKBitmap(decodeInfo);

        // A valid header is not sufficient. Decoding every pixel rejects truncated or corrupt
        // payloads that could otherwise be stored as apparently valid images.
        cancellationToken.ThrowIfCancellationRequested();
        var decodeResult = codec.GetPixels(decodeInfo, decodedBitmap.GetPixels());
        if (decodeResult != SKCodecResult.Success)
        {
            throw InvalidImage();
        }

        cancellationToken.ThrowIfCancellationRequested();
        SKBitmap? orientedBitmap = null;
        try
        {
            var bitmapToEncode = decodedBitmap;
            if (codec.EncodedOrigin != SKEncodedOrigin.TopLeft)
            {
                orientedBitmap = ApplyEncodedOrientation(decodedBitmap, codec.EncodedOrigin);
                bitmapToEncode = orientedBitmap;
            }

            // Re-encoding creates a canonical payload from decoded pixels. This prevents the
            // original container, trailing bytes, filename, or client MIME type from reaching
            // object storage.
            var content = Encode(bitmapToEncode, format);

            if (content.LongLength > MediaUploadLimits.MaxImageBytes)
            {
                throw new MediaProcessingException("The processed image cannot exceed 5 MB.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            return new ProcessedImage(content, format, bitmapToEncode.Width, bitmapToEncode.Height);
        }
        finally
        {
            orientedBitmap?.Dispose();
        }
    }

    /// <summary>
    /// Creates a codec from the encoded bytes and maps expected parse failures to the public
    /// invalid-image error without hiding operational failures such as missing native binaries.
    /// </summary>
    private static SKCodec CreateCodec(Stream source)
    {
        try
        {
            var codec = SKCodec.Create(source, out var result);
            if (codec is null || result != SKCodecResult.Success)
            {
                codec?.Dispose();
                throw InvalidImage();
            }

            return codec;
        }
        catch (MediaProcessingException)
        {
            throw;
        }
        catch (Exception exception)
            when (exception is ArgumentException or InvalidOperationException or IOException)
        {
            throw InvalidImage();
        }
    }

    /// <summary>
    /// Converts SkiaSharp's detected format into the product module's restricted format model.
    /// </summary>
    private static ImageFormat GetSupportedFormat(SKEncodedImageFormat encodedFormat)
    {
        return encodedFormat switch
        {
            SKEncodedImageFormat.Jpeg => ImageFormat.Jpeg,
            SKEncodedImageFormat.Png => ImageFormat.Png,
            SKEncodedImageFormat.Webp => ImageFormat.WebP,
            _ => throw new MediaProcessingException("Only JPEG, PNG, and WebP images are allowed."),
        };
    }

    /// <summary>
    /// Checks inexpensive metadata before allocating a decoded bitmap.
    /// </summary>
    private static void ValidateImageMetadata(SKCodec codec)
    {
        var width = codec.Info.Width;
        var height = codec.Info.Height;

        if (width <= 0 || height <= 0)
        {
            throw InvalidImage();
        }

        if (
            width > MediaUploadLimits.MaxImageWidth
            || height > MediaUploadLimits.MaxImageHeight
            || (long)width * height > MediaUploadLimits.MaxImagePixels
        )
        {
            throw new MediaProcessingException(
                $"Image dimensions cannot exceed {MediaUploadLimits.MaxImageWidth}x"
                    + $"{MediaUploadLimits.MaxImageHeight} pixels or "
                    + $"{MediaUploadLimits.MaxImagePixels:N0} total pixels."
            );
        }

        if (codec.FrameCount > 1)
        {
            throw new MediaProcessingException("Animated images are not supported.");
        }
    }

    /// <summary>
    /// Applies the encoded EXIF orientation to the pixels so the re-encoded file no longer
    /// depends on orientation metadata for correct display.
    /// </summary>
    private static SKBitmap ApplyEncodedOrientation(SKBitmap source, SKEncodedOrigin encodedOrigin)
    {
        var swapsDimensions =
            encodedOrigin
            is SKEncodedOrigin.LeftTop
                or SKEncodedOrigin.RightTop
                or SKEncodedOrigin.RightBottom
                or SKEncodedOrigin.LeftBottom;
        var width = swapsDimensions ? source.Height : source.Width;
        var height = swapsDimensions ? source.Width : source.Height;
        var destination = new SKBitmap(width, height, source.ColorType, source.AlphaType);

        using var canvas = new SKCanvas(destination);
        canvas.SetMatrix(CreateOrientationMatrix(encodedOrigin, source.Width, source.Height));
        canvas.DrawBitmap(source, 0, 0, new SKSamplingOptions());
        canvas.Flush();

        return destination;
    }

    /// <summary>
    /// Builds the affine transform for each of the eight EXIF orientation values.
    /// Orientations five through eight swap the output width and height.
    /// </summary>
    private static SKMatrix CreateOrientationMatrix(
        SKEncodedOrigin encodedOrigin,
        int width,
        int height
    )
    {
        return encodedOrigin switch
        {
            SKEncodedOrigin.TopLeft => CreateMatrix(1, 0, 0, 0, 1, 0),
            SKEncodedOrigin.TopRight => CreateMatrix(-1, 0, width, 0, 1, 0),
            SKEncodedOrigin.BottomRight => CreateMatrix(-1, 0, width, 0, -1, height),
            SKEncodedOrigin.BottomLeft => CreateMatrix(1, 0, 0, 0, -1, height),
            SKEncodedOrigin.LeftTop => CreateMatrix(0, 1, 0, 1, 0, 0),
            SKEncodedOrigin.RightTop => CreateMatrix(0, -1, height, 1, 0, 0),
            SKEncodedOrigin.RightBottom => CreateMatrix(0, -1, height, -1, 0, width),
            SKEncodedOrigin.LeftBottom => CreateMatrix(0, 1, 0, -1, 0, width),
            _ => CreateMatrix(1, 0, 0, 0, 1, 0),
        };
    }

    // SKMatrix represents:
    // x' = scaleX * x + skewX * y + transX
    // y' = skewY * x + scaleY * y + transY
    private static SKMatrix CreateMatrix(
        float scaleX,
        float skewX,
        float transX,
        float skewY,
        float scaleY,
        float transY
    )
    {
        return new SKMatrix
        {
            ScaleX = scaleX,
            SkewX = skewX,
            TransX = transX,
            SkewY = skewY,
            ScaleY = scaleY,
            TransY = transY,
            Persp0 = 0,
            Persp1 = 0,
            Persp2 = 1,
        };
    }

    /// <summary>
    /// Encodes only the normalized bitmap. Lossy formats use the module's fixed quality so
    /// callers cannot influence the stored representation.
    /// </summary>
    private static byte[] Encode(SKBitmap bitmap, ImageFormat format)
    {
        using var stream = new MemoryStream();
        var encodedFormat = format switch
        {
            ImageFormat.Jpeg => SKEncodedImageFormat.Jpeg,
            ImageFormat.Png => SKEncodedImageFormat.Png,
            ImageFormat.WebP => SKEncodedImageFormat.Webp,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
        };
        var quality = format == ImageFormat.Png ? 100 : LossyImageQuality;

        if (!bitmap.Encode(stream, encodedFormat, quality))
        {
            throw InvalidImage();
        }

        return stream.ToArray();
    }

    private static MediaProcessingException InvalidImage()
    {
        return new MediaProcessingException(
            "The uploaded file is not a valid JPEG, PNG, or WebP image."
        );
    }
}
