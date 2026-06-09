# FlowOps Runbook

This runbook contains the daily operational commands used to run, verify, troubleshoot, and present the FlowOps platform.

## 1. Current Project Version

Final stable version:

    v1.0-flowops-final

Onboarding documentation version:

    v1.0.1-docs-onboarding

Repository:

    https://github.com/Kareem-Waled/flowops-platform.git

## 2. Quick Health Check

Run these commands before any demo:

    git status --short
    kubectl get nodes
    kubectl get applications -n argocd
    kubectl get pods -n staging
    kubectl get pods -n production
    kubectl get pods -n monitoring
    helm list -n monitoring

Expected results:

    Git working tree is clean
    Kubernetes nodes are Ready
    ArgoCD applications are Synced and Healthy
    Application pods are Running
    Monitoring pods are Running
    Helm monitoring release is deployed

## 3. Access URLs

Get VM IP:

    hostname -I | awk '{print $1}'

Main URLs:

    Jenkins:    http://<VM_IP>:8081
    Grafana:    http://<VM_IP>:33000
    Prometheus: http://<VM_IP>:39090

Grafana login:

    Username: admin
    Password: admin123

## 4. Local Docker Compose

Start local runtime:

    docker compose up -d --build

Check containers:

    docker ps

Test services:

    curl http://localhost:5101/health
    curl http://localhost:5101/version
    curl http://localhost:5102/health
    curl http://localhost:5102/version

Stop local runtime:

    docker compose down

## 5. Kubernetes Checks

Check nodes:

    kubectl get nodes

Check staging:

    kubectl get pods -n staging
    kubectl get svc -n staging

Check production:

    kubectl get pods -n production
    kubectl get svc -n production

Check monitoring:

    kubectl get pods -n monitoring
    kubectl get svc -n monitoring

## 6. ArgoCD Checks

Check applications:

    kubectl get applications -n argocd

Expected:

    flowops-staging      Synced   Healthy
    flowops-production   Synced   Healthy

Refresh ArgoCD application manually if needed:

    kubectl annotate application flowops-staging -n argocd argocd.argoproj.io/refresh=hard --overwrite
    kubectl annotate application flowops-production -n argocd argocd.argoproj.io/refresh=hard --overwrite

## 7. Jenkins Operations

Start Jenkins container:

    docker start jenkins

Check Jenkins container:

    docker ps | grep jenkins

Open Jenkins:

    http://<VM_IP>:8081

Main jobs:

    flowops-smoke-build
    flowops-deploy-staging
    flowops-deploy-production

After running deployment jobs, verify ArgoCD and Kubernetes:

    kubectl get applications -n argocd
    kubectl get pods -n staging
    kubectl get pods -n production

## 8. Check Deployed Images

Staging images:

    kubectl get deployment user-service -n staging -o jsonpath='{.spec.template.spec.containers[0].image}{"\n"}'
    kubectl get deployment product-service -n staging -o jsonpath='{.spec.template.spec.containers[0].image}{"\n"}'

Production images:

    kubectl get deployment user-service -n production -o jsonpath='{.spec.template.spec.containers[0].image}{"\n"}'
    kubectl get deployment product-service -n production -o jsonpath='{.spec.template.spec.containers[0].image}{"\n"}'

Expected stable images:

    ahmednazir490/flowops-user-service:1
    ahmednazir490/flowops-product-service:1
    ahmednazir490/flowops-user-service:prod-1
    ahmednazir490/flowops-product-service:prod-1

## 9. Test Production Services

Open user service port-forward:

    kubectl port-forward -n production deployment/user-service 7101:8080

Test in another terminal:

    curl http://localhost:7101/health
    curl http://localhost:7101/version

Open product service port-forward:

    kubectl port-forward -n production deployment/product-service 7102:8080

Test in another terminal:

    curl http://localhost:7102/health
    curl http://localhost:7102/version

Stop port-forward:

    Ctrl + C

## 10. Monitoring Operations

Open Grafana and Prometheus:

    bash k8s/monitoring/open-monitoring.sh

Check Grafana health:

    curl http://localhost:33000/api/health

Check Prometheus readiness:

    curl http://localhost:39090/-/ready

Run Prometheus query from browser:

    up

## 11. Common Troubleshooting

### Port already in use

Find process:

    ss -tulpn | grep ':33000'
    ss -tulpn | grep ':39090'

Kill old port-forward:

    pkill -f 'kubectl port-forward.*33000' || true
    pkill -f 'kubectl port-forward.*39090' || true

Reopen monitoring:

    bash k8s/monitoring/open-monitoring.sh

### Grafana login failed

Check Grafana secret:

    kubectl get secret flowops-monitoring-grafana -n monitoring -o jsonpath='{.data.admin-user}' | base64 -d
    echo
    kubectl get secret flowops-monitoring-grafana -n monitoring -o jsonpath='{.data.admin-password}' | base64 -d
    echo

Expected:

    admin
    admin123

Reset password if needed:

    GRAFANA_POD=$(kubectl get pod -n monitoring -l app.kubernetes.io/name=grafana,app.kubernetes.io/instance=flowops-monitoring -o jsonpath='{.items[0].metadata.name}')

    kubectl exec -n monitoring "$GRAFANA_POD" -c grafana -- grafana-cli admin reset-admin-password admin123

### ImagePullBackOff

Check failing pod:

    kubectl get pods -n monitoring

Describe pod:

    kubectl describe pod <POD_NAME> -n monitoring

Load monitoring images:

    bash k8s/monitoring/load-monitoring-images.sh

Restart deployment if needed:

    kubectl rollout restart deployment/flowops-monitoring-grafana -n monitoring

### ArgoCD not syncing

Check app status:

    kubectl get applications -n argocd

Hard refresh:

    kubectl annotate application flowops-staging -n argocd argocd.argoproj.io/refresh=hard --overwrite
    kubectl annotate application flowops-production -n argocd argocd.argoproj.io/refresh=hard --overwrite

Check again:

    kubectl get applications -n argocd

## 12. Demo Flow

Recommended demo order:

    1. Show GitHub repository.
    2. Show Docker Compose local runtime.
    3. Show Kubernetes staging and production pods.
    4. Show ArgoCD applications are Synced and Healthy.
    5. Show Jenkins jobs.
    6. Run or explain Jenkins deployment flow.
    7. Show DockerHub images.
    8. Show Grafana.
    9. Show Prometheus query.
    10. Show final Git tags and release ZIP.

## 13. Release Commands

Check current tags:

    git tag -n

Create a new tag:

    git tag -a <TAG_NAME> -m '<TAG_MESSAGE>'

Push tag:

    git push origin <TAG_NAME>

Create ZIP from tag:

    git archive --format=zip --output='../FlowOps_<TAG_NAME>.zip' <TAG_NAME>

Verify ZIP does not include .git:

    unzip -l '../FlowOps_<TAG_NAME>.zip' | grep -E ' \.git/' || echo 'No .git directory found in ZIP'

## 14. Important Notes

Do not commit secrets.

Do not paste GitHub tokens or DockerHub passwords into documentation.

Terraform, cloud deployment, portal login page, and role-based access control are future work items and are not implemented in the current stable version.
