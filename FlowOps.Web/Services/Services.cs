using FlowOps.Core.Entities;
using FlowOps.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FlowOps.Web.Services;

// ── Mock GitHub ───────────────────────────────────────────
public class MockGitHubService : IGitHubService
{
    public Task<string> CommitDeploymentManifestAsync(string app,string env,string tag,int replicas,string msg,CancellationToken ct=default)
        => Task.FromResult("a3f9c12");
    public Task<string?> GetLatestCommitHashAsync(string branch="main",CancellationToken ct=default)
        => Task.FromResult<string?>("a3f9c12");
    public Task<bool> IsRepositoryHealthyAsync(CancellationToken ct=default)
        => Task.FromResult(true);
}

// ── Mock ArgoCD ───────────────────────────────────────────
public class MockArgoCDService : IArgoCDService
{
    private static readonly string[] Apps = {"user-service","product-service"};
    public Task<List<ArgoAppStatus>> GetAllAppStatusesAsync(CancellationToken ct=default) =>
        Task.FromResult(Apps.Select((a,i)=>new ArgoAppStatus{
            AppName=a, Environment=i==0?"production":"staging",
            SyncStatus=i==2?SyncStatus.Syncing:SyncStatus.Synced,
            HealthStatus=i==2?HealthStatus.Progressing:HealthStatus.Healthy,
            CommitHash="a3f9c12", LastSyncedAt=DateTime.UtcNow.AddMinutes(-(i*15+2))
        }).ToList());
    public Task<ArgoAppStatus?> GetAppStatusAsync(string appName,CancellationToken ct=default) =>
        Task.FromResult<ArgoAppStatus?>(new ArgoAppStatus{AppName=appName,SyncStatus=SyncStatus.Synced,HealthStatus=HealthStatus.Healthy,LastSyncedAt=DateTime.UtcNow});
    public Task<bool> ForceSyncAsync(string appName,CancellationToken ct=default) => Task.FromResult(true);
    public Task<bool> ForceSyncAllAsync(CancellationToken ct=default) => Task.FromResult(true);
}

// ── Mock Kubernetes ───────────────────────────────────────
public class MockKubernetesService : IKubernetesService
{
    private static readonly (string name,string env,string ver,int pods,AppStatus status)[] _apps =
    {
        ("user-service",    "production","v1.0.0",2,AppStatus.Running),
        ("product-service", "staging",   "v1.0.0",1,AppStatus.Running),
    };
    public Task<List<Application>> GetRunningApplicationsAsync(CancellationToken ct=default) =>
        Task.FromResult(_apps.Select(a=>new Application{Name=a.name,Environment=a.env,CurrentVersion=a.ver,ReplicaCount=a.pods,Status=a.status,Namespace=a.env,LastDeployedAt=DateTime.UtcNow.AddHours(-2)}).ToList());
    public Task<List<Pod>> GetPodsAsync(string? appName=null,string? ns=null,CancellationToken ct=default)
    {
        var pods = new List<Pod>
        {
            new()
            {
                Name = "user-service-pod",
                AppName = "user-service",
                Namespace = "production",
                Status = PodStatus.Running,
                StartedAt = DateTime.UtcNow.AddMinutes(-25),
                NodeName = "local-docker",
                CpuUsagePercent = 22,
                MemoryUsagePercent = 41
            },
            new()
            {
                Name = "product-service-pod",
                AppName = "product-service",
                Namespace = "staging",
                Status = PodStatus.Running,
                StartedAt = DateTime.UtcNow.AddMinutes(-20),
                NodeName = "local-docker",
                CpuUsagePercent = 18,
                MemoryUsagePercent = 38
            }
        };

        if (!string.IsNullOrWhiteSpace(appName))
            pods = pods.Where(pod => pod.AppName == appName).ToList();

        if (!string.IsNullOrWhiteSpace(ns))
            pods = pods.Where(pod => pod.Namespace == ns).ToList();

        return Task.FromResult(pods);
    }
    public Task<List<ClusterInfo>> GetClustersAsync(CancellationToken ct=default) =>
        Task.FromResult(new List<ClusterInfo>{
            new(){Name="local-docker",Region="local",Provider="Docker Compose",KubernetesVersion="n/a",Status=ClusterStatus.Healthy,TotalNodes=1,ReadyNodes=1,TotalPods=2,CpuUsagePercent=22,MemoryUsagePercent=41,ArgoSynced=true,TerraformManaged=false},
        });
    public Task<ClusterInfo?> GetClusterAsync(string name,CancellationToken ct=default) =>
        Task.FromResult<ClusterInfo?>(new ClusterInfo{Name=name,Region="us-east-1",Provider="Docker Compose",KubernetesVersion="n/a",Status=ClusterStatus.Healthy,TotalNodes=1,ReadyNodes=1,TotalPods=2,CpuUsagePercent=22,MemoryUsagePercent=41,ArgoSynced=true,TerraformManaged=false});
}

// ── Mock Prometheus ───────────────────────────────────────
public class MockPrometheusService : IPrometheusService
{
    private static readonly Random _rnd = new(42);
    public Task<double> GetCurrentCpuUsageAsync(string? a=null,CancellationToken ct=default)    => Task.FromResult(Math.Round(30+_rnd.NextDouble()*30,1));
    public Task<double> GetCurrentMemoryUsageAsync(string? a=null,CancellationToken ct=default) => Task.FromResult(Math.Round(50+_rnd.NextDouble()*30,1));
    public Task<double> GetRequestsPerSecondAsync(string? a=null,CancellationToken ct=default)  => Task.FromResult(Math.Round(200+_rnd.NextDouble()*150,1));
    public Task<MetricSeries> GetCpuTimeSeriesAsync(string? a=null,int minutes=10,CancellationToken ct=default) =>
        Task.FromResult(MakeSeries("cpu",minutes));
    public Task<MetricSeries> GetMemoryTimeSeriesAsync(string? a=null,int minutes=10,CancellationToken ct=default) =>
        Task.FromResult(MakeSeries("memory",minutes));
    public Task<List<Alert>> GetActiveAlertsAsync(CancellationToken ct=default) =>
        Task.FromResult(new List<Alert>{
            new(){AppName="user-service",   Severity=AlertSeverity.Info,    Message="Health check passed — all pods ready",FiredAt=DateTime.UtcNow.AddMinutes(-20),IsResolved=true},
        });
    private static MetricSeries MakeSeries(string name,int minutes)
    {
        var rnd=new Random(name.GetHashCode());
        return new MetricSeries{MetricName=name,Points=Enumerable.Range(0,minutes).Select(i=>new MetricPoint{Timestamp=DateTime.UtcNow.AddMinutes(-(minutes-i)),Value=Math.Round(30+rnd.NextDouble()*50,1)}).ToList()};
    }
}

// ── Deployment Service ────────────────────────────────────
public class DeploymentService : IDeploymentService
{
    private readonly IGitHubService _github;
    private readonly IArgoCDService _argocd;
    private readonly IKubernetesService _k8s;
    private readonly ILogger<DeploymentService> _logger;
    private static readonly List<Deployment> _history = new()
    {
        new(){AppName="user-service",    TargetVersion="v1.0.0",Environment="production",TriggeredBy="Kareem", Status=DeploymentStatus.Success,  StartedAt=DateTime.UtcNow.AddMinutes(-18), CompletedAt=DateTime.UtcNow.AddMinutes(-16),CommitHash="b7e2d44"},
        new(){AppName="product-service", TargetVersion="v1.0.0",Environment="staging",   TriggeredBy="Abdel Nasser", Status=DeploymentStatus.Success,  StartedAt=DateTime.UtcNow.AddHours(-1),   CompletedAt=DateTime.UtcNow.AddHours(-1).AddMinutes(2),CommitHash="e9c3f55"},
    };

    public DeploymentService(IGitHubService github,IArgoCDService argocd,IKubernetesService k8s,ILogger<DeploymentService> logger)
    { _github=github;_argocd=argocd;_k8s=k8s;_logger=logger; }

    public async Task<Deployment> TriggerDeploymentAsync(string appName,string environment,string imageTag,int replicas,DeployStrategy strategy,string triggeredBy,CancellationToken ct=default)
    {
        var dep=new Deployment{AppName=appName,TargetVersion=imageTag,Environment=environment,TriggeredBy=triggeredBy,Strategy=strategy,Replicas=replicas,Status=DeploymentStatus.Pending,StartedAt=DateTime.UtcNow};
        _history.Insert(0,dep);
        _=RunPipelineAsync(dep,ct);
        return dep;
    }

    public Task<List<Deployment>> GetDeploymentHistoryAsync(string? appName=null,string? environment=null,int take=50,CancellationToken ct=default)
    {
        var q=_history.AsEnumerable();
        if(appName!=null) q=q.Where(d=>d.AppName==appName);
        if(environment!=null) q=q.Where(d=>d.Environment==environment);
        return Task.FromResult(q.Take(take).ToList());
    }

    public Task<Deployment?> GetDeploymentAsync(string id,CancellationToken ct=default)
        =>Task.FromResult(_history.FirstOrDefault(d=>d.Id==id));

    private async Task RunPipelineAsync(Deployment dep,CancellationToken ct)
    {
        try
        {
            dep.Status=DeploymentStatus.Running;
            dep.CommitHash=await _github.CommitDeploymentManifestAsync(dep.AppName,dep.Environment,dep.TargetVersion,dep.Replicas,$"deploy({dep.AppName}): {dep.TargetVersion}",ct);
            await Task.Delay(2000,ct);
            dep.Status=DeploymentStatus.Success;
            dep.CompletedAt=DateTime.UtcNow;
            _logger.LogInformation("Deployment {Id} succeeded",dep.Id);
        }
        catch(Exception ex)
        {
            dep.Status=DeploymentStatus.Failed;
            dep.ErrorMessage=ex.Message;
            dep.CompletedAt=DateTime.UtcNow;
        }
    }
}
