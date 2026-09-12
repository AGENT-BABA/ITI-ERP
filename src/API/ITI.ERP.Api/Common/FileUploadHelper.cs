namespace ITI.ERP.Api.Common;

using System.IO.Compression;
public static class FileUploadHelper
{
    public const long MaxPhotoSize = 5 * 1024 * 1024;       // 5 MB
    public const long MaxExcelSize = 10 * 1024 * 1024;      // 10 MB

    // Magic byte signatures
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] WebpSignatureRiff = [0x52, 0x49, 0x46, 0x46]; // "RIFF"
    private static readonly byte[] WebpSignatureWebp = [0x57, 0x45, 0x42, 0x50]; // "WEBP" at offset 8
    private static readonly byte[] XlsxSignature = [0x50, 0x4B, 0x03, 0x04];     // ZIP/PK (XLSX is ZIP)
    private static readonly byte[] XlsSignature = [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1]; // OLE2

    public static (bool IsValid, string? Error) ValidatePhoto(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return (false, "No file uploaded.");

        if (file.Length > MaxPhotoSize)
            return (false, $"Photo must be ≤ {MaxPhotoSize / 1024 / 1024} MB.");

        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp"))
            return (false, "Only JPEG, PNG, and WebP images are allowed.");

        using var stream = file.OpenReadStream();
        var header = new byte[12];
        if (stream.Read(header, 0, 12) < 12)
            return (false, "File is too small to be a valid image.");

        var isValid = ext switch
        {
            ".jpg" or ".jpeg" => HasSignature(header, JpegSignature),
            ".png" => HasSignature(header, PngSignature),
            ".webp" => HasSignature(header, WebpSignatureRiff) &&
                       HasSignatureAt(header, 8, WebpSignatureWebp),
            _ => false
        };

        return isValid
            ? (true, null)
            : (false, "File content does not match its extension. Upload may be corrupted or tampered.");
    }
    public static (bool IsValid, string? Error) ValidateExcel(IFormFile file, bool allowXls = false)
    {
        if (file is null || file.Length == 0)
            return (false, "Please upload a file.");

        if (file.Length > MaxExcelSize)
            return (false, $"File must be ≤ {MaxExcelSize / 1024 / 1024} MB.");

        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext != ".xlsx" && (allowXls ? ext != ".xls" : true))
            return (false, allowXls ? "Only .xlsx and .xls files are supported." : "Only .xlsx files are supported.");

        using var stream = file.OpenReadStream();

        if (ext == ".xlsx")
        {
            // ZIP header + internal structure check
            var header = new byte[4];
            if (stream.Read(header, 0, 4) < 4)
                return (false, "File is too small to be a valid Excel file.");
            if (!HasSignature(header, XlsxSignature))
                return (false, "File content does not match .xlsx extension.");

            stream.Position = 0;
            try
            {
                using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
                if (archive.GetEntry("[Content_Types].xml") is null ||
                    archive.GetEntry("xl/workbook.xml") is null)
                    return (false, "File is not a valid .xlsx (missing internal structure).");
            }
            catch (Exception ex) when (ex is InvalidDataException or IOException)
            {
                return (false, "File is corrupted or not a valid .xlsx.");
            }
        }
        else // .xls
        {
            var header = new byte[8];
            if (stream.Read(header, 0, 8) < 8)
                return (false, "File is too small to be a valid Excel file.");
            if (!HasSignature(header, XlsSignature))
                return (false, "File content does not match .xls extension.");
        }

        return (true, null);
    }

    private static bool HasSignature(byte[] header, byte[] signature)
    {
        if (header.Length < signature.Length) return false;
        return signature.AsSpan().SequenceEqual(header.AsSpan(0, signature.Length));
    }

    private static bool HasSignatureAt(byte[] header, int offset, byte[] signature)
    {
        if (header.Length < offset + signature.Length) return false;
        return signature.AsSpan().SequenceEqual(header.AsSpan(offset, signature.Length));
    }
}