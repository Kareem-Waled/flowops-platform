variable "aws_region"   { type = string; default = "us-east-1" }
variable "environment"  { type = string; default = "production"; validation { condition = contains(["production","staging","development"],var.environment); error_message = "Must be production, staging, or development." } }
variable "project_name" { type = string; default = "flowops" }
variable "vpc_cidr"     { type = string; default = "10.0.0.0/16" }
variable "node_count"   { type = number; default = 3 }
variable "node_type"    { type = string; default = "t3.medium" }
variable "github_repo_url" { type = string }
variable "github_token"    { type = string; sensitive = true }
