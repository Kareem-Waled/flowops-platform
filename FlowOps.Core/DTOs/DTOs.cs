using FlowOps.Core.Entities;
namespace FlowOps.Core.DTOs;
public record DeploymentRequestDto(string AppName,string Environment,string ImageTag,int Replicas,DeployStrategy Strategy,string TriggeredBy);
public record DashboardSummaryDto(int RunningApps,int DeploymentsToday,int ActivePods,double AvgDeploySeconds,double CpuPercent,double MemoryPercent,double NetworkMbps,List<Application> Applications,List<Deployment> RecentDeployments);
public record MonitorSummaryDto(double UptimePercent,double CpuAvgPercent,double MemoryAvgPercent,double RequestsPerSecond,List<MetricPoint> CpuSeries,List<MetricPoint> MemorySeries,List<Alert> ActiveAlerts);
public record ArgoSummaryDto(int SyncedCount,int TotalCount,string LastSyncAgo,string? LatestCommitHash,string? LatestCommitAuthor,List<ArgoAppStatus> Apps);
