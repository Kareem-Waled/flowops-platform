using FlowOps.Core.Interfaces;

namespace FlowOps.Infrastructure.GitHub;

public class GitHubService : IGitHubService
{
    public Task<string> CommitDeploymentManifestAsync(string appName, string environment, string imageTag, int replicas, string commitMessage, CancellationToken ct = default)
        => Task.FromResult(Guid.NewGuid().ToString("N")[..7]);

    public Task<string?> GetLatestCommitHashAsync(string branch = "main", CancellationToken ct = default)
        => Task.FromResult<string?>("local-demo");

    public Task<bool> IsRepositoryHealthyAsync(CancellationToken ct = default)
        => Task.FromResult(true);
}
