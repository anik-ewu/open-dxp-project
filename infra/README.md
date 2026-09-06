# Deploying OpenDXP

Two deployment targets live here: Azure Container Apps (`main.bicep`) and Kubernetes
(`helm/opendxp`). Their status is very different — read the right section.

## Kubernetes (`helm/`) — deployed and verified on a real cluster

Unlike the Azure path below, this one has actually been installed and exercised end to end on a
local minikube cluster: login → create a draft → publish → the full Kafka fan-out (cache
invalidation, search re-index, auto-tagging, audit trail all confirmed via their real output, not
just "no errors in the log") → delivery API serving from Redis. The HPA reads real CPU metrics via
metrics-server. What it has *not* been run against is a real cloud cluster (AKS) — see "What's
still unverified" below.

Two real bugs turned up specifically from deploying, not from reading the YAML:

- **Redpanda crash-looped** with `Cannot assign requested address`. `--rpc-addr` was set to the
  Kubernetes Service DNS name, which resolves to a ClusterIP — not a real interface inside the
  pod's network namespace, so binding to it fails. The Service name belongs on
  `--advertise-rpc-addr` (what *other* clients resolve to reach this pod), a different concern
  from what the process binds locally (`0.0.0.0`).
- **Every API request got a 400** ("This server only accepts HTTPS requests") once
  `ASPNETCORE_ENVIRONMENT` was set to `Production`, because there's no real TLS termination in
  front of a local cluster. Fixed properly (not by relaxing the check) with ASP.NET Core's
  `ForwardedHeaders` middleware trusting `X-Forwarded-Proto` from the ingress — confirmed by
  testing the identical request with and without that header. `aspnetEnvironment` in `values.yaml`
  is configurable specifically so this stays a real Production requirement rather than getting
  quietly disabled for convenience.

### Try it locally

```bash
minikube start
minikube addons enable metrics-server

# Build and load the images minikube will run
docker build --target final -f src/OpenDXP.Api/Dockerfile -t opendxp-api:local .
docker build --target final -t opendxp-admin:local client/admin
minikube image load opendxp-api:local
minikube image load opendxp-admin:local

helm install opendxp infra/helm/opendxp \
  --set secrets.postgresPassword=<a-local-secret> \
  --set aspnetEnvironment=Development   # no TLS at the ingress locally - see the bug note above

kubectl get pods           # wait for everything to reach 1/1 (postgres + redpanda take longest)
kubectl port-forward svc/opendxp-api 18080:8080   # then hit http://localhost:18080/health
```

If you rebuild an image with the same tag, `minikube image load` alone won't refresh a running
pod — Kubernetes checks by tag, not content. Either bump the tag or force it:
`kubectl scale deployment/opendxp-api --replicas=0 && minikube image rm opendxp-api:local && minikube image load opendxp-api:local && kubectl scale deployment/opendxp-api --replicas=2`.

### What's still unverified

- **A real cloud cluster (AKS).** The ingress class (`nginx`), storage class (default, untested
  against Azure Disks), and plaintext `values.yaml` secret (should come from Azure Key Vault via
  the CSI driver, or at minimum `--set` at install time, never committed) all likely need
  adjustment. The chart's structure should carry over; the specifics haven't been tried.
- **Kafka's managed equivalent** — same gap as the Bicep template below.
- **TLS at the ingress** — `aspnetEnvironment: Production` is the correct default, but needs a
  real certificate (cert-manager + Let's Encrypt, or an Azure-managed cert on AKS) to actually work
  end to end rather than falling back to `Development` as done here for local testing.

## Azure Container Apps (`main.bicep`) — validated locally, not deployed

**Status: not deployed.** `az bicep build` compiles it to ARM JSON without errors, and both
production Docker images (`docker build --target final`) build and run correctly on this machine.
None of it has been run against a real Azure subscription — that needs real credentials and
explicit approval to spend money, neither of which this session has. This is the weaker-verified
of the two paths for exactly that reason; prefer the Kubernetes path above if you want to see
something that's actually been proven to work.

### What's here

- `main.bicep` — Container Apps environment, API + Admin container apps, Postgres Flexible Server
  (with the `vector` extension enabled for Phase 5's related-pages feature), Azure Cache for
  Redis, and a Container Registry to hold the built images.
- `client/admin/Dockerfile`'s `final` stage — an nginx-served production build of the Angular
  admin, reverse-proxying `/api`, `/connect`, `/account` to the API container. Verified locally to
  actually start (an earlier version crash-looped the same way as the Kubernetes nginx resolver
  bug above — fixed the same way, with a `resolver` + variable `proxy_pass`).

### What's explicitly not solved yet

- **Kafka.** No managed equivalent is wired into `main.bicep`. Azure Event Hubs exposes a
  Kafka-compatible endpoint and is the obvious candidate, but its topic/consumer-group semantics
  differ from Redpanda enough that swapping the connection string blind would be guessing, not
  engineering. This needs its own testing pass against a real Event Hubs namespace.
- **The nginx `resolver` IP** (`DNS_RESOLVER` build arg) defaults to Docker's embedded DNS
  (`127.0.0.11`) for local testing. Azure Container Apps needs its own resolver address here -
  unverified, since nothing has been deployed.
- **OpenTelemetry export target.** Locally this points at the Jaeger container. A real deployment
  needs a hosted trace backend (Azure Monitor / Application Insights is the natural fit given
  everything else is Azure) - not wired up.
- **TLS/custom domain, secrets rotation, CDN in front of the admin static assets** - none of this
  is in scope yet.

### If you do want to run this for real

```bash
# 1. Log in and pick a subscription
az login
az account set --subscription <subscription-id>

# 2. Create a resource group
az group create --name opendxp-rg --location eastus

# 3. Build and push images to a registry the deployment can pull from
#    (the Bicep template creates its own ACR, but you need to build+push after it exists -
#    this is a two-pass deploy: infra first, then images, then point the Container Apps at them)
az deployment group create \
  --resource-group opendxp-rg \
  --template-file infra/main.bicep \
  --parameters postgresAdminPassword=<a-real-secret>

# 4. Build and push the images (registry name comes from the deployment output)
az acr login --name <registry-name-from-output>
docker build --target final -f src/OpenDXP.Api/Dockerfile -t <registry>.azurecr.io/opendxp-api:latest .
docker push <registry>.azurecr.io/opendxp-api:latest
docker build --target final -t <registry>.azurecr.io/opendxp-admin:latest client/admin
docker push <registry>.azurecr.io/opendxp-admin:latest

# 5. Re-run the deployment (or `az containerapp update`) so the container apps pick up the images
```

This is intentionally not automated into a single command yet - the two-pass nature (infra needs
to exist before you can push images to *its* registry) is worth understanding before scripting
over it, and scripting it against a subscription nobody has tested with yet would be the wrong
place to start automating.
