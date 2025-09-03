// Services/IStorageService.cs
using Library81.Shared.DTOs;
using Library81.Shared.ViewModels;

namespace Library81.Services;

public interface IStorageService
{
    Task<ApiResponse<T>> SaveAsync<T>(string key, T data);
    Task<ApiResponse<T>> LoadAsync<T>(string key);
    Task<ApiResponse<bool>> SyncToCloudAsync();
    Task<ApiResponse<bool>> SyncFromCloudAsync();
    Task<ApiResponse<bool>> ExportBackupAsync(string filePath);
    Task<ApiResponse<bool>> ImportBackupAsync(string filePath);
    StorageMode CurrentMode { get; }
}

public enum StorageMode
{
    LocalOnly,
    CloudOnly,
    HybridSync,
    OfflineFirst
}