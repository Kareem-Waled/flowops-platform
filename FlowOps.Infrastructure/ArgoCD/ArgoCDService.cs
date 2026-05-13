using FlowOps.Core.Entities;
using FlowOps.Core.Interfaces;

namespace FlowOps.Infrastructure.ArgoCD;

public class ArgoCDService : IArgoCDService
{
    private static readonly string[] Apps = ["user-service", "product-service"];

    public Task<List<ArgoAppStatus>> GetAllAppStatusesAsync(CancellationToken ct = default)
        => Task.FromResult(Apps.Select(app => new ArgoAppStatus
        {
            AppName = app,
            Environment = "staging",
            SyncStatus = SyncStatus.Synced,
            HealthStatus = HealthStatus.Healthy,
            CommitHash = "local-demo",
            LastSyncedAt = DateTime.UtcNow.AddMinutes(-2)
        }).ToList());

    public Task<ArgoAppStatus?> GetAppStatusAsync(string appName, CancellationToken ct = default)
        => Task.FromResult<ArgoAppStatus?>(new ArgoAppStatus
        {
            AppName = appName,
            Environment = "staging",
            SyncStatus = SyncStatus.Synced,
            HealthStatus = HealthStatus.Healthy,
            CommitHash = "local-demo",
            LastSyncedAt = DateTime.UtcNow
        });

    public Task<bool> ForceSyncAsync(string appName, CancellationToken ct = default) => Task.FromResult(true);
    public Task<bool> ForceSyncAllAsync(CancellationToken ct = default) => Task.FromResult(true);
}
