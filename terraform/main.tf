terraform {
  required_version = ">= 1.6.0"
  required_providers {
    aws        = { source = "hashicorp/aws",        version = "~> 5.0" }
    kubernetes = { source = "hashicorp/kubernetes",  version = "~> 2.25" }
    helm       = { source = "hashicorp/helm",        version = "~> 2.12" }
  }
  backend "s3" {
    bucket         = "flowops-terraform-state"
    key            = "flowops/terraform.tfstate"
    region         = "us-east-1"
    encrypt        = true
    dynamodb_table = "flowops-state-lock"
  }
}
provider "aws" {
  region = var.aws_region
  default_tags { tags = { Project = "FlowOps", ManagedBy = "Terraform", Environment = var.environment } }
}
provider "kubernetes" {
  host                   = module.eks.cluster_endpoint
  cluster_ca_certificate = base64decode(module.eks.cluster_ca_certificate)
  token                  = module.eks.cluster_token
}
provider "helm" {
  kubernetes {
    host                   = module.eks.cluster_endpoint
    cluster_ca_certificate = base64decode(module.eks.cluster_ca_certificate)
    token                  = module.eks.cluster_token
  }
}
module "vpc"        { source = "./modules/vpc";        environment = var.environment; aws_region = var.aws_region; vpc_cidr = var.vpc_cidr }
module "eks"        { source = "./modules/eks";        environment = var.environment; cluster_name = "${var.project_name}-${var.environment}"; vpc_id = module.vpc.vpc_id; private_subnets = module.vpc.private_subnet_ids; node_count = var.node_count; node_type = var.node_type }
module "argocd"     { source = "./modules/argocd";     environment = var.environment; cluster_endpoint = module.eks.cluster_endpoint; github_repo_url = var.github_repo_url; github_token = var.github_token; depends_on = [module.eks] }
module "prometheus" { source = "./modules/prometheus";  environment = var.environment; depends_on = [module.eks] }
