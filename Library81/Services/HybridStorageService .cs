// Services/HybridStorageService.cs
using Library81.Models;
using Library81.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Library81.Services;

public class HybridStorageService : IStorageService
{
    private readonly LibraryContext _mysqlContext;
    private readonly LocalDbContext _localContext;
    private readonly ILogger<HybridStorageService> _logger;
    private readonly IConfiguration _configuration;

    public StorageMode CurrentMode { get; private set; }

    public HybridStorageService(
        LibraryContext mysqlContext,
        LocalDbContext localContext,
        ILogger<HybridStorageService> logger,
        IConfiguration configuration)
    {
        _mysqlContext = mysqlContext;
        _localContext = localContext;
        _logger = logger;
        _configuration = configuration;

        // Détermine le mode de stockage au démarrage
        CurrentMode = DetermineStorageMode();
    }

    public async Task<ApiResponse<T>> SaveAsync<T>(string key, T data)
    {
        try
        {
            var success = true;
            var errors = new List<string>();

            // Sauvegarde locale (toujours)
            await SaveToLocal(key, data);

            // Sauvegarde cloud selon le mode
            if (CurrentMode == StorageMode.HybridSync || CurrentMode == StorageMode.CloudOnly)
            {
                try
                {
                    await SaveToCloud(key, data);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Cloud save failed, continuing with local only");
                    errors.Add($"Cloud sync failed: {ex.Message}");
                    CurrentMode = StorageMode.LocalOnly;
                }
            }

            return new ApiResponse<T>
            {
                Success = success,
                Data = data,
                Errors = errors
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in hybrid save");
            return new ApiResponse<T>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<T>> LoadAsync<T>(string key)
    {
        try
        {
            // Tentative de chargement local d'abord (plus rapide)
            var localData = await LoadFromLocal<T>(key);

            if (CurrentMode == StorageMode.LocalOnly || localData != null)
            {
                return new ApiResponse<T>
                {
                    Success = true,
                    Data = localData
                };
            }

            // Si pas de données locales, essayer le cloud
            var cloudData = await LoadFromCloud<T>(key);

            // Si trouvé dans le cloud, sauver localement pour la prochaine fois
            if (cloudData != null)
            {
                await SaveToLocal(key, cloudData);
            }

            return new ApiResponse<T>
            {
                Success = true,
                Data = cloudData ?? localData
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in hybrid load");
            return new ApiResponse<T>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<bool>> SyncToCloudAsync()
    {
        if (!await IsCloudAvailable())
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Cloud not available"
            };
        }

        try
        {
            // Récupérer toutes les données modifiées localement
            var localBooks = await _localContext.Books
                .Include(b => b.Item)
                .Where(b => b.Item.DateAdded > GetLastSyncDate())
                .ToListAsync();

            foreach (var book in localBooks)
            {
                await SaveToCloud($"book_{book.BookId}", book);
            }

            await UpdateLastSyncDate();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = $"Synced {localBooks.Count} items to cloud"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing to cloud");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<bool>> ExportBackupAsync(string filePath)
    {
        try
        {
            var backupData = new
            {
                ExportDate = DateTime.UtcNow,
                Books = await _localContext.Books
                    .Include(b => b.Item)
                    .ThenInclude(i => i.Subcategory)
                    .Select(b => new
                    {
                        b.BookId,
                        b.Isbn,
                        b.GenreId,
                        Item = new
                        {
                            b.Item.Title,
                            b.Item.Creator,
                            b.Item.Publisher,
                            b.Item.Year,
                            b.Item.Description,
                            b.Item.ImageUrl,
                            b.Item.DateAdded
                        }
                    }).ToListAsync(),
                Categories = await _localContext.Categories.ToListAsync(),
                Genres = await _localContext.Genres.ToListAsync()
            };

            var json = JsonSerializer.Serialize(backupData, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(filePath, json);

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = $"Backup exported to {filePath}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting backup");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    private StorageMode DetermineStorageMode()
    {
        var mode = _configuration["StorageMode"];

        return mode?.ToLower() switch
        {
            "local" => StorageMode.LocalOnly,
            "cloud" => StorageMode.CloudOnly,
            "offline" => StorageMode.OfflineFirst,
            _ => StorageMode.HybridSync
        };
    }

    private async Task SaveToLocal<T>(string key, T data)
    {
        var json = JsonSerializer.Serialize(data);
        var localData = new LocalStorage
        {
            Key = key,
            Data = json,
            LastModified = DateTime.UtcNow
        };

        var existing = await _localContext.LocalStorages
            .FirstOrDefaultAsync(ls => ls.Key == key);

        if (existing != null)
        {
            existing.Data = json;
            existing.LastModified = DateTime.UtcNow;
        }
        else
        {
            _localContext.LocalStorages.Add(localData);
        }

        await _localContext.SaveChangesAsync();
    }

    private async Task<T?> LoadFromLocal<T>(string key)
    {
        var localData = await _localContext.LocalStorages
            .FirstOrDefaultAsync(ls => ls.Key == key);

        if (localData?.Data == null) return default;

        return JsonSerializer.Deserialize<T>(localData.Data);
    }

    private async Task SaveToCloud<T>(string key, T data)
    {
        // Implémentation selon votre choix de cloud
        // (Azure Blob, AWS S3, Google Drive API, etc.)
        await Task.Delay(100); // Placeholder
    }

    private async Task<T?> LoadFromCloud<T>(string key)
    {
        // Implémentation selon votre choix de cloud
        await Task.Delay(100); // Placeholder
        return default;
    }

    private async Task<bool> IsCloudAvailable()
    {
        try
        {
            // Test de connectivité
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await client.GetAsync("https://google.com");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private DateTime GetLastSyncDate()
    {
        var setting = _localContext.Settings
            .FirstOrDefault(s => s.Key == "LastSyncDate");

        return setting != null ?
            DateTime.Parse(setting.Value) :
            DateTime.MinValue;
    }

    private async Task UpdateLastSyncDate()
    {
        var setting = await _localContext.Settings
            .FirstOrDefaultAsync(s => s.Key == "LastSyncDate");

        if (setting != null)
        {
            setting.Value = DateTime.UtcNow.ToString();
        }
        else
        {
            _localContext.Settings.Add(new Setting
            {
                Key = "LastSyncDate",
                Value = DateTime.UtcNow.ToString()
            });
        }

        await _localContext.SaveChangesAsync();
    }

    public async Task<ApiResponse<bool>> SyncFromCloudAsync()
    {
        // Implémentation similaire à SyncToCloudAsync mais dans l'autre sens
        throw new NotImplementedException();
    }

    public async Task<ApiResponse<bool>> ImportBackupAsync(string filePath)
    {
        // Implémentation de l'import
        throw new NotImplementedException();
    }
}