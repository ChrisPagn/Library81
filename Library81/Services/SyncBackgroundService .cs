// Library81/Services/SyncBackgroundService.cs
namespace Library81.Services;

public class SyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SyncBackgroundService> _logger;
    private readonly IConfiguration _configuration;

    public SyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<SyncBackgroundService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = _configuration.GetValue<int>("SyncSettings:AutoSyncIntervalMinutes", 30);
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(interval));

        while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var storageService = scope.ServiceProvider.GetRequiredService<IStorageService>();

                var result = await storageService.SyncToCloudAsync();

                if (result.Success)
                {
                    _logger.LogInformation("Synchronisation automatique réussie: {Message}", result.Message);
                }
                else
                {
                    _logger.LogWarning("Échec de la synchronisation automatique: {Message}", result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la synchronisation automatique");
            }
        }
    }
}