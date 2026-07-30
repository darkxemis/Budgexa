namespace Budgexa.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveProfileImageAsync(Guid userId, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task DeleteProfileImageAsync(string fileUrl, CancellationToken cancellationToken = default);
    Task<string> SaveSignatureImageAsync(Guid userId, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task DeleteSignatureImageAsync(string fileUrl, CancellationToken cancellationToken = default);
    Task<byte[]?> GetSignatureImageBytesAsync(string? fileUrl, CancellationToken cancellationToken = default);
}
