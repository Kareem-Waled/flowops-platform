# FlowOps Demo Steps

1. Start Minikube.
2. Start Jenkins container.
3. Run Docker Compose locally.
4. Verify user-service and product-service locally.
5. Show Kubernetes staging and production namespaces.
6. Show ArgoCD applications are Synced and Healthy.
7. Run Jenkins smoke build job.
8. Run Jenkins staging deployment job.
9. Verify staging images and pods.
10. Run Jenkins production deployment job.
11. Verify production images and pods.
12. Open Grafana and Prometheus.
13. Run Prometheus query: up.
14. Show monitoring pods are Running.

Useful commands:

    kubectl get applications -n argocd
    kubectl get pods -n staging
    kubectl get pods -n production
    kubectl get pods -n monitoring
    helm list -n monitoring

Demo URLs:

    Jenkins:    http://<VM_IP>:8081
    Grafana:    http://<VM_IP>:33000
    Prometheus: http://<VM_IP>:39090
