# FlowOps Setup Guide

This guide explains how a new team member can prepare the environment and run the FlowOps project.

## 1. Project Repository

Repository:

    https://github.com/Kareem-Waled/flowops-platform.git

Clone the repository:

    git clone https://github.com/Kareem-Waled/flowops-platform.git
    cd flowops-platform

Current stable release:

    v1.0-flowops-final

## 2. Required Tools

Install and verify the following tools:

    git
    docker
    docker compose
    kubectl
    minikube
    helm

Check versions:

    git --version
    docker --version
    docker compose version
    kubectl version --client
    minikube version
    helm version

## 3. Local Docker Compose Run

Build and start the local application:

    docker compose up -d --build

Verify containers:

    docker ps

Test services:

    curl http://localhost:5101/health
    curl http://localhost:5101/version
    curl http://localhost:5102/health
    curl http://localhost:5102/version

Open the portal:

    http://localhost:5000

Stop local environment:

    docker compose down

## 4. Kubernetes Environment

Start Minikube:

    minikube start

Check nodes:

    kubectl get nodes

Expected result:

    minikube
    minikube-m02
    minikube-m03

If a single-node Minikube is used, the project can still run, but the original demo was validated on a three-node Minikube cluster.

## 5. Kubernetes Namespaces

Apply base namespaces:

    kubectl apply -f k8s/manifests/base/namespaces.yaml

Check namespaces:

    kubectl get namespaces

Expected namespaces:

    staging
    production
    monitoring

## 6. ArgoCD

The project includes ArgoCD application manifests:

    k8s/argocd/applications/flowops-staging.yaml
    k8s/argocd/applications/flowops-production.yaml

If ArgoCD is already installed, apply the applications:

    kubectl apply -f k8s/argocd/applications/flowops-staging.yaml
    kubectl apply -f k8s/argocd/applications/flowops-production.yaml

Check ArgoCD applications:

    kubectl get applications -n argocd

Expected result:

    flowops-staging      Synced   Healthy
    flowops-production   Synced   Healthy

## 7. Jenkins

Jenkins is used for CI/CD automation.

Jenkins files:

    ci/jenkins/Dockerfile
    ci/jenkins/run-jenkins.sh
    ci/jenkins/jobs/flowops-smoke-build.sh
    ci/jenkins/jobs/flowops-deploy-staging.sh
    ci/jenkins/jobs/flowops-deploy-production.sh

Start Jenkins using the project script:

    bash ci/jenkins/run-jenkins.sh

Open Jenkins:

    http://localhost:8081

From another machine on the same network:

    http://<VM_IP>:8081

Main Jenkins jobs:

    flowops-smoke-build
    flowops-deploy-staging
    flowops-deploy-production

## 8. Jenkins Required Credentials

Jenkins needs access to:

    GitHub repository
    DockerHub registry
    Local Docker daemon
    Kubernetes cluster through kubectl

Do not commit tokens or passwords to GitHub.

The original project used local Jenkins secrets for:

    GitHub token
    DockerHub login

Each team member should configure their own credentials locally.

## 9. DockerHub Images

The current stable images are:

Staging:

    ahmednazir490/flowops-user-service:1
    ahmednazir490/flowops-product-service:1

Production:

    ahmednazir490/flowops-user-service:prod-1
    ahmednazir490/flowops-product-service:prod-1

Check deployed images:

    kubectl get deployment user-service -n staging -o jsonpath='{.spec.template.spec.containers[0].image}{"\n"}'
    kubectl get deployment product-service -n staging -o jsonpath='{.spec.template.spec.containers[0].image}{"\n"}'
    kubectl get deployment user-service -n production -o jsonpath='{.spec.template.spec.containers[0].image}{"\n"}'
    kubectl get deployment product-service -n production -o jsonpath='{.spec.template.spec.containers[0].image}{"\n"}'

## 10. Monitoring Setup

Monitoring files:

    k8s/monitoring/values.yaml
    k8s/monitoring/load-monitoring-images.sh
    k8s/monitoring/install-monitoring.sh
    k8s/monitoring/open-monitoring.sh

Load images into Minikube:

    bash k8s/monitoring/load-monitoring-images.sh

Install monitoring:

    bash k8s/monitoring/install-monitoring.sh

Open Grafana and Prometheus:

    bash k8s/monitoring/open-monitoring.sh

Grafana login:

    Username: admin
    Password: admin123

Default URLs:

    Grafana:    http://<VM_IP>:33000
    Prometheus: http://<VM_IP>:39090

## 11. Final Verification

Run:

    kubectl get nodes
    kubectl get applications -n argocd
    kubectl get pods -n staging
    kubectl get pods -n production
    kubectl get pods -n monitoring
    helm list -n monitoring

Expected status:

    Kubernetes nodes are Ready
    ArgoCD apps are Synced and Healthy
    Staging pods are Running
    Production pods are Running
    Monitoring pods are Running
    Helm monitoring release is deployed

## 12. Notes

This project is currently implemented on a local Ubuntu VMware environment using Minikube.

Terraform, cloud deployment, portal login, and role-based access control are not implemented in the current final version.

These items are documented as future work and should only be described as implemented after they are actually added to the project.
