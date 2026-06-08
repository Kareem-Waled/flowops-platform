#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(git rev-parse --show-toplevel 2>/dev/null || pwd)"

echo "===== Install FlowOps Monitoring ====="

helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo update

kubectl create namespace monitoring --dry-run=client -o yaml | kubectl apply -f -

helm upgrade --install flowops-monitoring prometheus-community/kube-prometheus-stack \
  --namespace monitoring \
  -f "$ROOT_DIR/k8s/monitoring/values.yaml" \
  --timeout 10m

echo
echo "===== Monitoring pods ====="
kubectl get pods -n monitoring

echo
echo "===== Monitoring services ====="
kubectl get svc -n monitoring
