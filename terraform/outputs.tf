output "cluster_name"       { value = module.eks.cluster_name }
output "cluster_endpoint"   { value = module.eks.cluster_endpoint }
output "argocd_url"         { value = module.argocd.argocd_url }
output "prometheus_url"     { value = module.prometheus.prometheus_url }
output "grafana_url"        { value = module.prometheus.grafana_url }
output "kubeconfig_command" { value = "aws eks update-kubeconfig --region ${var.aws_region} --name ${module.eks.cluster_name}" }
