#!/usr/bin/env bash
set -e

echo "===== Jenkins FlowOps Smoke Build ====="

REPO_URL="https://github.com/Kareem-Waled/flowops-platform.git"
WORK_DIR="$WORKSPACE/flowops-platform"

echo "Cleaning workspace..."
rm -rf "$WORK_DIR"

echo "Cloning repo..."
git clone "$REPO_URL" "$WORK_DIR"

cd "$WORK_DIR"

echo "Current commit:"
git log --oneline -1

echo "Docker version:"
docker --version

export DOCKER_BUILDKIT=0

echo "Building user-service image..."
docker build -t flowops-jenkins-user-service:smoke services/user-service

echo "Building product-service image..."
docker build -t flowops-jenkins-product-service:smoke services/product-service

echo "Building flowops-portal image..."
docker build -t flowops-jenkins-portal:smoke .

echo "Built images:"
docker images | grep -E "flowops-jenkins-user-service|flowops-jenkins-product-service|flowops-jenkins-portal"

echo "===== Smoke Build Completed Successfully ====="
