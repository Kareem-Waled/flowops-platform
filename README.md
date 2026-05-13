# FlowOps — GitOps Developer Self-Service Platform

FlowOps is a DevOps graduation project that demonstrates a GitOps-based self-service deployment platform.

This fixed version is simplified to **2 microservices** so it can run locally first:

- `user-service`
- `product-service`

The portal uses mock integrations in local development, so you can run the UI without a real Kubernetes cluster.

## Local Run with Docker Compose

```bash
docker compose up --build
```

Open:

```text
Portal:          http://localhost:5000
User Service:    http://localhost:5101/version
Product Service: http://localhost:5102/version
```

## Local Run without Docker

```bash
cd FlowOps.Web
dotnet run --urls http://localhost:5000
```

In separate terminals:

```bash
cd services/user-service
dotnet run --urls http://localhost:5101
```

```bash
cd services/product-service
dotnet run --urls http://localhost:5102
```

## Project Structure

```text
FlowOps.Core/          Domain entities + interfaces
FlowOps.Infrastructure/ Stub integrations for GitHub, ArgoCD, Kubernetes, Prometheus
FlowOps.Web/           ASP.NET Core MVC portal
services/user-service/ User microservice
services/product-service/ Product microservice
k8s/manifests/         Kubernetes manifests for production and staging
.github/workflows/     CI/CD workflow examples
```

## Next DevOps Steps

1. Run locally.
2. Build Docker images.
3. Push images to a container registry.
4. Deploy manifests to Kubernetes.
5. Install ArgoCD.
6. Let ArgoCD sync from the GitOps repository.
