namespace FlowOps.Core.Entities;
public class Application { public string Name{get;set;}=""; public string Environment{get;set;}=""; public string CurrentVersion{get;set;}=""; public int ReplicaCount{get;set;} public AppStatus Status{get;set;} public DateTime LastDeployedAt{get;set;} public string DeployedBy{get;set;}=""; public string Namespace{get;set;}=""; }
public class Deployment { public string Id{get;set;}=Guid.NewGuid().ToString(); public string AppName{get;set;}=""; public string TargetVersion{get;set;}=""; public string Environment{get;set;}=""; public string TriggeredBy{get;set;}=""; public DeploymentStatus Status{get;set;} public DeployStrategy Strategy{get;set;} public int Replicas{get;set;}=3; public DateTime StartedAt{get;set;}=DateTime.UtcNow; public DateTime? CompletedAt{get;set;} public string? CommitHash{get;set;} public string? ErrorMessage{get;set;} public TimeSpan? Duration=>CompletedAt.HasValue?CompletedAt-StartedAt:null; }
public class Pod { public string Name{get;set;}=""; public string AppName{get;set;}=""; public string Namespace{get;set;}=""; public PodStatus Status{get;set;} public DateTime StartedAt{get;set;} public double CpuUsagePercent{get;set;} public double MemoryUsagePercent{get;set;} public string NodeName{get;set;}=""; public int RestartCount{get;set;} }
public class ClusterInfo { public string Name{get;set;}=""; public string Region{get;set;}=""; public string Provider{get;set;}=""; public string KubernetesVersion{get;set;}=""; public ClusterStatus Status{get;set;} public int TotalNodes{get;set;} public int ReadyNodes{get;set;} public int TotalPods{get;set;} public double CpuUsagePercent{get;set;} public double MemoryUsagePercent{get;set;} public bool ArgoSynced{get;set;} public bool TerraformManaged{get;set;} }
public class ArgoAppStatus { public string AppName{get;set;}=""; public string Environment{get;set;}=""; public SyncStatus SyncStatus{get;set;} public HealthStatus HealthStatus{get;set;} public string? CommitHash{get;set;} public DateTime? LastSyncedAt{get;set;} public string? SyncError{get;set;} }
public class MetricSeries { public string MetricName{get;set;}=""; public List<MetricPoint> Points{get;set;}=new(); }
public class MetricPoint { public DateTime Timestamp{get;set;} public double Value{get;set;} }
public class Alert { public string Id{get;set;}=Guid.NewGuid().ToString(); public string AppName{get;set;}=""; public AlertSeverity Severity{get;set;} public string Message{get;set;}=""; public DateTime FiredAt{get;set;} public bool IsResolved{get;set;} public DateTime? ResolvedAt{get;set;} }
public enum AppStatus{Running,Deploying,Failed,Stopped}
public enum DeploymentStatus{Pending,Running,Success,Failed,RolledBack}
public enum DeployStrategy{Rolling,BlueGreen,Canary}
public enum PodStatus{Running,Pending,Failed,Terminating}
public enum ClusterStatus{Healthy,Degraded,Unreachable}
public enum SyncStatus{Synced,Syncing,OutOfSync,Unknown}
public enum HealthStatus{Healthy,Progressing,Degraded,Missing}
public enum AlertSeverity{Info,Warning,Critical}
