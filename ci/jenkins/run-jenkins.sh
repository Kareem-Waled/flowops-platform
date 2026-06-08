#!/usr/bin/env bash
set -euo pipefail

IMAGE_NAME="flowops-jenkins-docker:local"
CONTAINER_NAME="jenkins"
JENKINS_VOLUME="jenkins_home"

HOST_DOCKER="$(command -v docker)"
DOCKER_GID="$(stat -c '%g' /var/run/docker.sock)"

echo "Building Jenkins image..."
docker build -t "$IMAGE_NAME" ci/jenkins

echo "Replacing existing Jenkins container if exists..."
docker rm -f "$CONTAINER_NAME" 2>/dev/null || true

echo "Starting Jenkins..."
docker run -d \
  --name "$CONTAINER_NAME" \
  --restart unless-stopped \
  -p 8081:8080 \
  -p 50000:50000 \
  -v "$JENKINS_VOLUME":/var/jenkins_home \
  -v /var/run/docker.sock:/var/run/docker.sock \
  -v "$HOST_DOCKER":/usr/bin/docker:ro \
  --group-add "$DOCKER_GID" \
  "$IMAGE_NAME"

docker ps --filter name="$CONTAINER_NAME"
