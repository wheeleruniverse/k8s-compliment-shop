# Action Items

## Before First Deployment

### Required Changes

- [ ] **Update ArgoCD repository URL**
  - File: `k8s/argocd/application.yaml`
  - Line: `repoURL: https://github.com/YOUR-ORG/k8s-compliment-shop.git`
  - Change `YOUR-ORG` to your actual GitHub organization/username

### Installation Prerequisites

- [ ] **Install Helm**
  ```bash
  # macOS
  brew install helm

  # Linux
  curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash
  ```

- [ ] **Install Kustomize**
  ```bash
  # macOS
  brew install kustomize

  # Linux
  curl -s "https://raw.githubusercontent.com/kubernetes-sigs/kustomize/master/hack/install_kustomize.sh" | bash
  ```

- [ ] **Install kubectl** (if not already installed)
  ```bash
  # macOS
  brew install kubectl
  ```

### Testing

- [ ] **Test Helm chart locally**
  ```bash
  cd /path/to/k8s-compliment-shop
  helm lint k8s/helm
  helm template k8s-compliment-shop k8s/helm --debug
  ```

- [ ] **Test vendoring script**
  ```bash
  ./scripts/vendor-manifests.sh
  cat k8s/rendered/all-resources.yaml
  ```

- [ ] **Validate manifests**
  ```bash
  kubectl apply --dry-run=client -f k8s/rendered/all-resources.yaml
  ```

### ArgoCD Setup

- [ ] **Install ArgoCD in your cluster**
  ```bash
  kubectl create namespace argocd
  kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml
  ```

- [ ] **Access ArgoCD UI**
  ```bash
  # Get admin password
  kubectl -n argocd get secret argocd-initial-admin-secret \
    -o jsonpath="{.data.password}" | base64 -d && echo

  # Port forward
  kubectl port-forward svc/argocd-server -n argocd 8080:443

  # Open https://localhost:8080
  ```

- [ ] **Deploy ArgoCD Application**
  ```bash
  kubectl apply -f k8s/argocd/application.yaml
  ```

- [ ] **Sync the application**
  ```bash
  argocd app sync k8s-compliment-shop
  ```

### Verification

- [ ] **Check all pods are running**
  ```bash
  kubectl get pods
  ```

- [ ] **Check services are created**
  ```bash
  kubectl get services
  ```

- [ ] **Test web service**
  ```bash
  kubectl port-forward svc/web-service 8080:8080
  # Open http://localhost:8080
  ```

- [ ] **Verify ArgoCD sync status**
  ```bash
  argocd app get k8s-compliment-shop
  ```

## Optional Enhancements

### Security

- [ ] **Use proper secret management**
  - Replace hardcoded MySQL password in `values.yaml`
  - Use Google Secret Manager, External Secrets Operator, or Sealed Secrets

- [ ] **Enable security patches**
  ```bash
  # Rename example patches
  cd k8s/helm/kustomize/patches/
  mv security-context.yaml.example security-context.yaml

  # Enable in kustomization.yaml
  # Add to patchesStrategicMerge:
  #   - patches/security-context.yaml
  ```

### Monitoring

- [ ] **Enable monitoring labels**
  ```bash
  # Rename example patch
  cd k8s/helm/kustomize/patches/
  mv add-monitoring-labels.yaml.example add-monitoring-labels.yaml

  # Enable in kustomization.yaml
  ```

- [ ] **Set up Prometheus monitoring**
- [ ] **Set up Grafana dashboards**

### Multi-Environment

- [ ] **Create environment-specific values files**
  - `k8s/helm/values-dev.yaml`
  - `k8s/helm/values-staging.yaml`
  - `k8s/helm/values-production.yaml`

- [ ] **Create separate ArgoCD Applications**
  - `k8s/argocd/application-dev.yaml`
  - `k8s/argocd/application-staging.yaml`
  - `k8s/argocd/application-production.yaml`

### CI/CD

- [ ] **Set up GitHub Actions secrets** (if needed)
- [ ] **Configure ArgoCD notifications**
  - Slack notifications for sync failures
  - Email notifications for deployments

- [ ] **Set up branch protection**
  - Require PR reviews
  - Require status checks (vendoring workflow)

### Cleanup

- [ ] **Remove old K8s manifests** (after verifying new setup works)
  ```bash
  git rm -r services/web-service/k8s/
  git rm -r services/bff-service/k8s/
  git rm -r services/product-service/k8s/
  git rm -r infrastructure/mysql/
  git commit -m "chore: remove old K8s manifests"
  ```

## Documentation Review

- [ ] Read `k8s/README.md`
- [ ] Read `k8s/QUICKSTART.md`
- [ ] Read `k8s/MIGRATION.md`
- [ ] Read `k8s/argocd/README.md`
- [ ] Read `k8s/helm/kustomize/README.md`

## Learning Resources

- [ ] [Helm Documentation](https://helm.sh/docs/)
- [ ] [Kustomize Documentation](https://kustomize.io/)
- [ ] [ArgoCD Documentation](https://argo-cd.readthedocs.io/)
- [ ] [GitOps Principles](https://www.gitops.tech/)
- [ ] [Kubernetes Best Practices](https://kubernetes.io/docs/concepts/configuration/overview/)

## Support Checklist

If you run into issues, check:

- [ ] Helm chart lints: `helm lint k8s/helm`
- [ ] Templates render: `helm template k8s-compliment-shop k8s/helm`
- [ ] Vendoring works: `./scripts/vendor-manifests.sh`
- [ ] ArgoCD logs: `kubectl logs -n argocd -l app.kubernetes.io/name=argocd-application-controller`
- [ ] Application logs: `kubectl logs -l app=web-service`
- [ ] ArgoCD sync status: `argocd app get k8s-compliment-shop`

## Notes

- **Do not commit sensitive data** - Use secret management solutions
- **Review rendered manifests** in PRs before merging
- **Test in dev/staging** before deploying to production
- **Use Git tags** to version your releases
- **Keep documentation updated** as you make changes

---

**Next Step**: Start with "Before First Deployment" section above!
