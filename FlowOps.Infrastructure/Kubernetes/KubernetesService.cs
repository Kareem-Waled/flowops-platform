using FlowOps.Core.Entities;
using FlowOps.Core.Interfaces;

namespace FlowOps.Infrastructure.Kubernetes;

public class KubernetesService : IKubernetesService
{
    public Task<List<Application>> GetRunningApplicationsAsync(CancellationToken ct = default)
        => Task.FromResult(new List<Application>
        {
            new(){ Name="user-service", Environment="staging", CurrentVersion="v1", ReplicaCount=1, Status=AppStatus.Running, Namespace="flowops-staging", LastDeployedAt=DateTime.UtcNow.AddMinutes(-20), DeployedBy="FlowOps" },
            new(){ Name="product-service", Environment="staging", CurrentVersion="v1", ReplicaCount=1, Status=AppStatus.Running, Namespace="flowops-staging", LastDeployedAt=DateTime.UtcNow.AddMinutes(-15), DeployedBy="FlowOps" }
        });

    public Task<List<Pod>> GetPodsAsync(string? appName = null, string? ns = null, CancellationToken ct = default)
    {
        var apps = appName is null ? new[] { "user-service", "product-service" } : new[] { appName };
        return Task.FromResult(apps.SelectMany(a => Enumerable.Range(1, 1).Select(i => new Pod
        {
            Name = $"{a}-{i}", AppName = a, Namespace = ns ?? "flowops-staging", Status = PodStatus.Running,
            StartedAt = DateTime.UtcNow.AddMinutes(-30), NodeName = "local-node", CpuUsagePercent = 18 + i, MemoryUsagePercent = 35 + i
        })).ToList());
    }

    public Task<List<ClusterInfo>> GetClustersAsync(CancellationToken ct = default)
        => Task.FromResult(new List<ClusterInfo>
        {
            new(){ Name="local-docker", Region="local", Provider="Docker Compose", KubernetesVersion="n/a", Status=ClusterStatus.Healthy, TotalNodes=1, ReadyNodes=1, TotalPods=2, CpuUsagePercent=22, MemoryUsagePercent=41, ArgoSynced=true, TerraformManaged=false }
        });

    public Task<ClusterInfo?> GetClusterAsync(string clusterName, CancellationToken ct = default)
        => Task.FromResult<ClusterInfo?>(new ClusterInfo{ Name=clusterName, Region="local", Provider="Docker Compose", KubernetesVersion="n/a", Status=ClusterStatus.Healthy, TotalNodes=1, ReadyNodes=1, TotalPods=2, CpuUsagePercent=22, MemoryUsagePercent=41, ArgoSynced=true, TerraformManaged=false });
}
