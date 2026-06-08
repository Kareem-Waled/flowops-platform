#!/usr/bin/env bash
set -euo pipefail

IMAGES=(
  "quay.io/prometheus-operator/prometheus-config-reloader:v0.91.0"
  "quay.io/prometheus/alertmanager:v0.32.2"
  "quay.io/kiwigrid/k8s-sidecar:2.7.3"
  "grafana/grafana:10.4.3"
  "registry.k8s.io/kube-state-metrics/kube-state-metrics:v2.19.0"
)

echo "===== Pull Monitoring images on host Docker ====="
for img in "${IMAGES[@]}"; do
  echo
  echo "Pulling: $img"
  docker pull "$img"
done

echo
echo "===== Load Monitoring images into Minikube nodes ====="
for img in "${IMAGES[@]}"; do
  echo
  echo "Loading into Minikube: $img"
  minikube image load --daemon=true --overwrite=true "$img"
done

echo
echo "===== Verify images on Minikube nodes ====="
for node in minikube minikube-m02 minikube-m03; do
  echo
  echo "----- $node -----"
  minikube ssh -n "$node" -- docker images | grep -E "prometheus-config-reloader|alertmanager|k8s-sidecar|grafana|kube-state-metrics" || true
done
