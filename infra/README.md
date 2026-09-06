# Deploying OpenDXP to Azure

**Status: not deployed.** Everything here has been validated locally - `az bicep build` compiles
`main.bicep` to ARM JSON without errors, and both production Docker images (`docker build --target
final`) build and run correctly on this machine. None of it has been run against a real Azure
subscription. That's a deliberate stopping point: doing so needs real Azure credentials and
explicit approval to spend money, neither of which this session has.

## What's here

- `main.bicep` — Container Apps environment, API + Admin container apps, Postgres Flexible Server
  (with the `vector` extension enabled for Phase 5's related-pages feature), Azure Cache for
  Redis, and a Container Registry to hold the built images.
- `client/admin/Dockerfile`'s `final` stage — an nginx-served production build of the Angular
  admin, reverse-proxying `/api`, `/connect`, `/account` to the API container. Verified locally to
  actually start (an earlier version crash-looped: nginx resolves `proxy_pass` hostnames once at
  config load and refuses to start if the host isn't resolvable yet — fixed with a `resolver` +
  variable `proxy_pass`, which defers resolution to request time).
- `src/OpenDXP.Api/Dockerfile`'s `final` stage — from Phase 0, already correct.

## What's explicitly not solved yet

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

## If you do want to run this for real

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
