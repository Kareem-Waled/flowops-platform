#!/usr/bin/env bash
set -euo pipefail

GRAFANA_PORT="${GRAFANA_PORT:-33000}"
PROMETHEUS_PORT="${PROMETHEUS_PORT:-39090}"
VM_IP="$(hostname -I | awk '{print $1}')"

echo "===== Open Grafana ====="
kill "$(cat /tmp/grafana-33000-pf.pid)" 2>/dev/null || true
rm -f /tmp/grafana-33000-pf.pid /tmp/grafana-33000-pf.log

kubectl port-forward --address 0.0.0.0 \
  -n monitoring svc/flowops-monitoring-grafana \
  "${GRAFANA_PORT}:80" >/tmp/grafana-33000-pf.log 2>&1 &

echo $! > /tmp/grafana-33000-pf.pid

echo "===== Open Prometheus ====="
kill "$(cat /tmp/prometheus-pf.pid)" 2>/dev/null || true
rm -f /tmp/prometheus-pf.pid /tmp/prometheus-pf.log

kubectl port-forward --address 0.0.0.0 \
  -n monitoring svc/flowops-monitoring-kube-pr-prometheus \
  "${PROMETHEUS_PORT}:9090" >/tmp/prometheus-pf.log 2>&1 &

echo $! > /tmp/prometheus-pf.pid

sleep 5

echo
echo "Grafana local:"
echo "http://localhost:${GRAFANA_PORT}"

echo
echo "Grafana VM IP:"
echo "http://${VM_IP}:${GRAFANA_PORT}"

echo
echo "Prometheus local:"
echo "http://localhost:${PROMETHEUS_PORT}"

echo
echo "Prometheus VM IP:"
echo "http://${VM_IP}:${PROMETHEUS_PORT}"

echo
echo "Grafana login:"
echo "Username: admin"
echo "Password: admin123"
