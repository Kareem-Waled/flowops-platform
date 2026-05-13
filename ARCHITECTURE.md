# FlowOps Architecture — 2 Microservices

```text
Developer
   │
   ├── pushes code to GitHub
   │
   ├── CI builds Docker images
   │      ├── user-service
   │      └── product-service
   │
   └── opens FlowOps Portal
          │
          ├── selects service + environment + image tag
          │
          ├── updates GitOps manifests
          │
          └── ArgoCD syncs Kubernetes
                 │
                 ├── user-service pods
                 └── product-service pods
```

## Local Development

In local development, the portal uses mock data for GitHub, ArgoCD, Kubernetes, and Prometheus.
This makes the UI and deployment flow testable before connecting a real cluster.

## Microservices

| Service | Port | Endpoints |
|---|---:|---|
| user-service | 5101 | `/health`, `/ready`, `/version`, `/api/users` |
| product-service | 5102 | `/health`, `/ready`, `/version`, `/api/products` |
| FlowOps Portal | 5000 | Dashboard, Deploy, Monitor, History, ArgoCD, Clusters |
