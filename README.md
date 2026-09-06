# OpenDXP

A headless CMS + Digital Experience Platform, built incrementally to learn (and demonstrate) auth/authorization best practices, event-driven architecture with Kafka, DXP concepts (personalization, experimentation), and AI-driven content features — end to end from a bare API to a Kubernetes deployment.

See [ARCHITECTURE.md](ARCHITECTURE.md) for how it fits together and why — event flow diagrams, the transactional outbox rationale, the auth flow, and a few implementation details (pgvector + EF Core 8) that cost real debugging time.

## Stack

- **Backend:** .NET 8, Clean Architecture (Domain / Application / Infrastructure / Api), CQRS via MediatR
- **Auth:** ASP.NET Core Identity + OpenIddict (OAuth2/OIDC authorization code + PKCE flow)
- **Frontend:** Angular 20 (admin UI), angular-oauth2-oidc
- **Data:** PostgreSQL, Redis
- **Messaging:** Kafka via Redpanda (transactional outbox + fan-out consumers)
- **AI:** pgvector (real cosine-similarity search; embeddings are a deterministic hashing placeholder pending an LLM/embeddings API key - see Phase 5)
- **Infra:** Docker Compose (local dev), Kubernetes via Helm (deployed and verified on a local cluster - `infra/helm`), Azure Container Apps IaC (validated, not yet deployed - `infra/main.bicep`)

## Project layout

```
src/
  OpenDXP.Domain/         # entities, value objects, domain events
  OpenDXP.Application/    # use cases, CQRS commands/queries (MediatR)
  OpenDXP.Infrastructure/ # EF Core, Redis, Kafka producers/consumers, external services
  OpenDXP.Api/             # ASP.NET Core Web API, controllers, auth
tests/
  OpenDXP.UnitTests/
client/
  admin/                  # Angular admin UI
```

## Local development

```bash
docker compose up
```

- API: http://localhost:8080
- Admin UI: http://localhost:4200
- Postgres: localhost:5432
- Redis: localhost:6379
- Kafka (Redpanda): localhost:19092
- Redpanda Console (topic/message browser): http://localhost:8090
- Jaeger (trace viewer): http://localhost:16686
- Metrics (Prometheus format): http://localhost:8080/metrics

## Build roadmap

Each phase is a working, demoable increment.

- [x] **Phase 0 — Foundation.** Clean Architecture solution, Angular admin shell, Docker Compose, CI.
- [x] **Phase 1 — Core CMS.** Content types/blocks/pages, draft→publish workflow with versioning, REST content delivery API.
- [x] **Phase 2 — Auth & Authorization.** OIDC login via OpenIddict (authorization code + PKCE), access/refresh token rotation, RBAC → resource-based ownership policy, account lockout + rate limiting, audit log. (MFA and multi-tenant claims deferred — not needed yet with a single admin team.)
- [x] **Phase 3 — Event-driven backbone.** Kafka (Redpanda) with transactional outbox; PagePublished domain event drives three independent consumers - Redis cache invalidation, search re-indexing, and audit trail - decoupled from the write path entirely.
- [x] **Phase 4 — DXP: personalization & experimentation.** PageVariant targeting by audience segment with priority ordering, stable per-visitor A/B traffic splitting (SHA256 bucketing), delivery-time selection via PersonalizationEngine, and an analytics dashboard (impressions/conversions/rate) fed by a fourth Kafka consumer aggregating VariantServed/ConversionRecorded events.
- [x] **Phase 5 — AI-adjacent features (no paid APIs yet).** Rule-based auto-tagging (stop-word-filtered keyword frequency, fifth Kafka consumer) and a real pgvector cosine-similarity "related pages" query, backed by a deterministic hashing-trick embedding rather than an LLM embedding model. Both are behind pluggable interfaces (`IEmbeddingService`) so a real provider (OpenAI/Voyage AI) drops in later. The LLM-backed "AI content assistant" is deferred until an API key is available.
- [x] **Phase 6 — Polish.** OpenTelemetry tracing (Jaeger) + metrics (`/metrics`), a domain-level `ActivitySource`/`Meter` that keeps Application/Infrastructure free of any OTel package reference, [ARCHITECTURE.md](ARCHITECTURE.md), and Azure deployment IaC in `infra/` - validated locally (Bicep syntax, both production Docker images built and smoke-tested) but deliberately not run against a live subscription (needs real credentials + spend approval).
- [x] **Phase 7 — Kubernetes.** Helm chart (API + Admin Deployments, Postgres StatefulSet + PVC, Redis, Redpanda, ConfigMap/Secret, Ingress, HPA on the API) - installed and exercised end to end on a real cluster (local minikube): login → publish → full Kafka fan-out → delivery from cache, all confirmed via actual output. Two real bugs found by deploying rather than reading YAML (Redpanda bind-vs-advertise address; HTTPS enforcement needing `ForwardedHeaders` behind an ingress) - both fixed, not worked around. AKS itself is the one piece left unverified (needs real Azure credentials).
