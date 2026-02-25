using BugPredictionBackend.Services.Sync;

namespace BugPredictionBackend.Background;

public class SonarSyncHostedService(IServiceScopeFactory scopeFactory, ILogger<SonarSyncHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan SyncInterval = TimeSpan.FromHours(6);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("SonarSyncHostedService started. Sync interval: {Interval}h", SyncInterval.TotalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunSyncAsync();
            await Task.Delay(SyncInterval, stoppingToken);
        }
    }

    private async Task RunSyncAsync()
    {
        logger.LogInformation("Scheduled sync starting at {Time}", DateTime.UtcNow);

        using IServiceScope scope = scopeFactory.CreateScope();
        SyncService syncService = scope.ServiceProvider.GetRequiredService<SyncService>();

        await syncService.SyncAllProjectsAsync();

        logger.LogInformation("Scheduled sync completed at {Time}", DateTime.UtcNow);
    }
}
