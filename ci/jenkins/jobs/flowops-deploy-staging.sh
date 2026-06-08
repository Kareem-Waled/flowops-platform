#!/usr/bin/env bash
set -euo pipefail

echo "===== Jenkins FlowOps Staging Deploy ====="

REPO_URL="https://github.com/Kareem-Waled/flowops-platform.git"
WORK_DIR="$WORKSPACE/flowops-platform"

DOCKERHUB_NAMESPACE="ahmednazir490"

BUILD_TAG_VALUE="${BUILD_NUMBER:-manual-$(date +%Y%m%d%H%M%S)}"

USER_IMAGE="${DOCKERHUB_NAMESPACE}/flowops-user-service:${BUILD_TAG_VALUE}"
PRODUCT_IMAGE="${DOCKERHUB_NAMESPACE}/flowops-product-service:${BUILD_TAG_VALUE}"

GITHUB_TOKEN_FILE="/var/jenkins_home/flowops-secrets/github_token"

echo "Build tag: ${BUILD_TAG_VALUE}"

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

echo "Building user-service image: $USER_IMAGE"
docker build -t "$USER_IMAGE" services/user-service

echo "Building product-service image: $PRODUCT_IMAGE"
docker build -t "$PRODUCT_IMAGE" services/product-service

echo "Pushing user-service image..."
docker push "$USER_IMAGE"

echo "Pushing product-service image..."
docker push "$PRODUCT_IMAGE"

echo "Updating Kubernetes staging manifests..."

sed -i "s|image: .*user-service:.*|image: ${USER_IMAGE}|g" \
  k8s/manifests/staging/user-service/deployment.yaml

sed -i "s|image: .*product-service:.*|image: ${PRODUCT_IMAGE}|g" \
  k8s/manifests/staging/product-service/deployment.yaml

echo "Updated image lines:"
grep -RIn "image:" k8s/manifests/staging/user-service/deployment.yaml k8s/manifests/staging/product-service/deployment.yaml

echo "Git diff:"
git diff -- k8s/manifests/staging

git config user.email "jenkins@flowops.local"
git config user.name "Jenkins CI"

git add k8s/manifests/staging/user-service/deployment.yaml
git add k8s/manifests/staging/product-service/deployment.yaml

if git diff --cached --quiet; then
  echo "No manifest changes to commit."
else
  git commit -m "Deploy staging images ${BUILD_TAG_VALUE}"

  echo "Pushing manifest changes to GitHub..."
  GITHUB_TOKEN="$(cat "$GITHUB_TOKEN_FILE")"
  git push "https://x-access-token:${GITHUB_TOKEN}@github.com/Kareem-Waled/flowops-platform.git" main
fi

echo "===== Jenkins FlowOps Staging Deploy Completed ====="
