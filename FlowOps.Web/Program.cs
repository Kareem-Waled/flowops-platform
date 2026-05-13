using FlowOps.Core.Interfaces;
using FlowOps.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var useMocks = builder.Environment.IsDevelopment() || string.Equals(builder.Configuration["FlowOps:UseMocks"], "true", StringComparison.OrdinalIgnoreCase);

if (useMocks)
{
    builder.Services.AddSingleton<IGitHubService, MockGitHubService>();
    builder.Services.AddSingleton<IArgoCDService, MockArgoCDService>();
    builder.Services.AddSingleton<IKubernetesService, MockKubernetesService>();
    builder.Services.AddSingleton<IPrometheusService, MockPrometheusService>();
}
else
{
    builder.Services.AddSingleton<IGitHubService, FlowOps.Infrastructure.GitHub.GitHubService>();
    builder.Services.AddSingleton<IArgoCDService, FlowOps.Infrastructure.ArgoCD.ArgoCDService>();
    builder.Services.AddSingleton<IKubernetesService, FlowOps.Infrastructure.Kubernetes.KubernetesService>();
    builder.Services.AddSingleton<IPrometheusService, FlowOps.Infrastructure.Prometheus.PrometheusService>();
}

builder.Services.AddSingleton<IDeploymentService, DeploymentService>();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Dashboard/Error");
    app.UseHsts();
}

// Disabled for Docker local demo
// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.MapHealthChecks("/health");
app.Run();
