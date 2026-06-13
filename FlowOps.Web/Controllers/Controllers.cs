using FlowOps.Core.DTOs;
using FlowOps.Core.Entities;
using FlowOps.Core.Interfaces;
using FlowOps.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowOps.Web.Controllers;

public class DashboardController : Controller
{
    private readonly IKubernetesService _k8s;
    private readonly IPrometheusService _prom;
    private readonly IDeploymentService _deploys;
    public DashboardController(IKubernetesService k8s,IPrometheusService prom,IDeploymentService deploys){_k8s=k8s;_prom=prom;_deploys=deploys;}

    public async Task<IActionResult> Index()
    {
        var apps   = await _k8s.GetRunningApplicationsAsync();
        var recent = await _deploys.GetDeploymentHistoryAsync(take:5);
        var cpu    = await _prom.GetCurrentCpuUsageAsync();
        var mem    = await _prom.GetCurrentMemoryUsageAsync();
        var net    = await _prom.GetRequestsPerSecondAsync();
        var avgSec = recent.Where(d=>d.Duration.HasValue).Select(d=>d.Duration!.Value.TotalSeconds).DefaultIfEmpty(134).Average();
        var summary= new DashboardSummaryDto(apps.Count(a=>a.Status==AppStatus.Running),recent.Count(d=>d.StartedAt.Date==DateTime.UtcNow.Date),apps.Sum(a=>a.ReplicaCount),avgSec,cpu,mem,net,apps,recent);
        return View(summary);
    }

    [HttpGet("/api/dashboard/metrics")]
    public async Task<IActionResult> LiveMetrics()
    {
        var cpu=await _prom.GetCurrentCpuUsageAsync();
        var mem=await _prom.GetCurrentMemoryUsageAsync();
        var rps=await _prom.GetRequestsPerSecondAsync();
        return Json(new{cpu,mem,rps,updatedAt=DateTime.UtcNow});
    }

    public IActionResult Error() => View();
}

public class DeployController : Controller
{
    private readonly IDeploymentService _deploys;
    private readonly IKubernetesService _k8s;
    public DeployController(IDeploymentService deploys,IKubernetesService k8s){_deploys=deploys;_k8s=k8s;}

    public async Task<IActionResult> Index()
    {
        ViewBag.Apps   = await _k8s.GetRunningApplicationsAsync();
        ViewBag.Recent = await _deploys.GetDeploymentHistoryAsync(take:3);
        return View();
    }

    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> Trigger([FromForm] DeploymentRequestDto request)
    {
        var dep=await _deploys.TriggerDeploymentAsync(request.AppName,request.Environment,request.ImageTag,request.Replicas,request.Strategy,request.TriggeredBy);
        return Json(new{deploymentId=dep.Id,status=dep.Status.ToString()});
    }

    [HttpGet("/api/deploy/{id}/status")]
    public async Task<IActionResult> Status(string id)
    {
        var dep=await _deploys.GetDeploymentAsync(id);
        if(dep is null) return NotFound();
        return Json(new{dep.Status,dep.CommitHash,dep.ErrorMessage,Duration=dep.Duration?.TotalSeconds});
    }
}

public class MonitorController : Controller
{
    private readonly IPrometheusService _prom;
    public MonitorController(IPrometheusService prom)=>_prom=prom;

    public async Task<IActionResult> Index()
    {
        var cpu=await _prom.GetCurrentCpuUsageAsync();
        var mem=await _prom.GetCurrentMemoryUsageAsync();
        var rps=await _prom.GetRequestsPerSecondAsync();
        var cpuS=await _prom.GetCpuTimeSeriesAsync(minutes:10);
        var memS=await _prom.GetMemoryTimeSeriesAsync(minutes:10);
        var alerts=await _prom.GetActiveAlertsAsync();
        return View(new MonitorSummaryDto(99.97,cpu,mem,rps,cpuS.Points,memS.Points,alerts));
    }

    [HttpGet("/api/monitor/series")]
    public async Task<IActionResult> Series()
    {
        var cpu=await _prom.GetCpuTimeSeriesAsync(minutes:10);
        var mem=await _prom.GetMemoryTimeSeriesAsync(minutes:10);
        return Json(new{cpu=cpu.Points,memory=mem.Points});
    }

    [HttpGet("/api/monitor/alerts")]
    public async Task<IActionResult> Alerts()
    {
        var alerts=await _prom.GetActiveAlertsAsync();
        return Json(new{critical=alerts.Count(a=>a.Severity==AlertSeverity.Critical&&!a.IsResolved)});
    }
}

public class HistoryController : Controller
{
    private readonly IDeploymentService _deploys;
    public HistoryController(IDeploymentService deploys)=>_deploys=deploys;

    public async Task<IActionResult> Index(string? app,string? env,string? status)
    {
        var all=await _deploys.GetDeploymentHistoryAsync(app,env,take:50);
        if(!string.IsNullOrEmpty(status)&&Enum.TryParse<DeploymentStatus>(status,out var s)) all=all.Where(d=>d.Status==s).ToList();
        ViewBag.FilterApp=app; ViewBag.FilterEnv=env; ViewBag.FilterStatus=status;
        return View(all);
    }
}

public class EnvironmentsController : Controller
{
    private readonly IKubernetesService _k8s;
    public EnvironmentsController(IKubernetesService k8s)=>_k8s=k8s;
    public async Task<IActionResult> Index(){return View(await _k8s.GetClustersAsync());}
}

public class ArgoCDController : Controller
{
    private readonly IArgoCDService _argo;
    private readonly IGitHubService _github;
    public ArgoCDController(IArgoCDService argo,IGitHubService github){_argo=argo;_github=github;}

    public async Task<IActionResult> Index()
    {
        var apps   =await _argo.GetAllAppStatusesAsync();
        var commit =await _github.GetLatestCommitHashAsync();
        var healthy=await _github.IsRepositoryHealthyAsync();
        var synced =apps.Count(a=>a.SyncStatus==SyncStatus.Synced);
        ViewBag.RepoHealthy=healthy;
        return View(new ArgoSummaryDto(synced,apps.Count,"2m ago",commit,"Team",apps));
    }

    [HttpPost("/api/argocd/sync/{appName}")]
    public async Task<IActionResult> ForceSync(string appName){return Json(new{success=await _argo.ForceSyncAsync(appName)});}

    [HttpPost("/api/argocd/sync-all")]
    public async Task<IActionResult> ForceSyncAll(){return Json(new{success=await _argo.ForceSyncAllAsync()});}
}

public class ClustersController : Controller
{
    private readonly IKubernetesService _k8s;
    public ClustersController(IKubernetesService k8s)=>_k8s=k8s;
    public async Task<IActionResult> Index(){return View(await _k8s.GetClustersAsync());}
    public async Task<IActionResult> Inspect(string name)
    {
        var cluster=await _k8s.GetClusterAsync(name);
        if(cluster is null) return NotFound();
        ViewBag.Pods=await _k8s.GetPodsAsync();
        return View(cluster);
    }
}

public class SettingsController : Controller
{
    public IActionResult Index()=>View();
    [HttpPost][ValidateAntiForgeryToken]
    public IActionResult Save(){TempData["Success"]="Settings saved successfully";return RedirectToAction(nameof(Index));}
}

public record PortalLogEvent(DateTime Time, string Level, string Source, string Message);

public class LogsController : Controller
{
    private readonly IDeploymentService _deploys;
    private readonly IKubernetesService _k8s;
    private readonly IArgoCDService _argo;
    private readonly IPrometheusService _prom;

    public LogsController(
        IDeploymentService deploys,
        IKubernetesService k8s,
        IArgoCDService argo,
        IPrometheusService prom)
    {
        _deploys = deploys;
        _k8s = k8s;
        _argo = argo;
        _prom = prom;
    }

    public async Task<IActionResult> Index()
    {
        var events = new List<PortalLogEvent>();
        var now = DateTime.UtcNow;

        events.Add(new PortalLogEvent(now, "INFO", "Portal", "FlowOps Logs Viewer loaded from live platform services."));

        try
        {
            var deployments = await _deploys.GetDeploymentHistoryAsync(take: 5);
            foreach (var dep in deployments)
            {
                var duration = dep.Duration.HasValue ? $" Duration: {dep.Duration.Value.TotalSeconds:F0}s." : "";
                events.Add(new PortalLogEvent(
                    dep.StartedAt,
                    dep.Status == DeploymentStatus.Failed ? "ERROR" : "INFO",
                    "Deployment",
                    $"Deployment record status: {dep.Status}.{duration}"
                ));
            }
        }
        catch (Exception ex)
        {
            events.Add(new PortalLogEvent(now, "WARN", "Deployment", $"Could not read deployment history: {ex.Message}"));
        }

        try
        {
            var apps = await _k8s.GetRunningApplicationsAsync();
            var running = apps.Count(a => a.Status == AppStatus.Running);
            var replicas = apps.Sum(a => a.ReplicaCount);

            events.Add(new PortalLogEvent(
                now.AddSeconds(-10),
                running > 0 ? "INFO" : "WARN",
                "Kubernetes",
                $"Kubernetes reports {running} running applications with {replicas} active replicas."
            ));
        }
        catch (Exception ex)
        {
            events.Add(new PortalLogEvent(now, "WARN", "Kubernetes", $"Could not read Kubernetes state: {ex.Message}"));
        }

        try
        {
            var argoApps = await _argo.GetAllAppStatusesAsync();
            var synced = argoApps.Count(a => a.SyncStatus == SyncStatus.Synced);
            var healthy = argoApps.Count(a => a.HealthStatus == HealthStatus.Healthy);

            events.Add(new PortalLogEvent(
                now.AddSeconds(-20),
                synced == argoApps.Count && healthy == argoApps.Count ? "INFO" : "WARN",
                "ArgoCD",
                $"ArgoCD reports {synced}/{argoApps.Count} applications Synced and {healthy}/{argoApps.Count} Healthy."
            ));
        }
        catch (Exception ex)
        {
            events.Add(new PortalLogEvent(now, "WARN", "ArgoCD", $"Could not read ArgoCD state: {ex.Message}"));
        }

        try
        {
            var alerts = await _prom.GetActiveAlertsAsync();
            var critical = alerts.Count(a => a.Severity == AlertSeverity.Critical && !a.IsResolved);

            events.Add(new PortalLogEvent(
                now.AddSeconds(-30),
                critical == 0 ? "INFO" : "ERROR",
                "Monitoring",
                critical == 0
                    ? "Prometheus reports no active critical alerts."
                    : $"Prometheus reports {critical} active critical alerts."
            ));
        }
        catch (Exception ex)
        {
            events.Add(new PortalLogEvent(now, "WARN", "Monitoring", $"Could not read Prometheus alerts: {ex.Message}"));
        }

        events = events
            .OrderByDescending(e => e.Time)
            .Take(12)
            .ToList();

        return View(events);
    }
}
