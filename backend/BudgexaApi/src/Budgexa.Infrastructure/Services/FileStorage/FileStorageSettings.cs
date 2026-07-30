namespace Budgexa.Infrastructure.Services.FileStorage;

public sealed class FileStorageSettings
{
    public const string SectionName = "FileStorage";

    public string ProfileImagesPath { get; set; } = "uploads/profile-images";
    public string SignatureImagesPath { get; set; } = "uploads/signature-images";
    public string BaseUrl { get; set; } = string.Empty;
}
