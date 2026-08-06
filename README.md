# OpenDXP

A headless CMS + Digital Experience Platform, built incrementally to learn (and demonstrate) auth/authorization best practices, event-driven architecture with Kafka, DXP concepts (personalization, experimentation), and AI-driven content features — end to end from a bare API to a Kubernetes deployment.

## Stack

- **Backend:** .NET 8, Clean Architecture (Domain / Application / Infrastructure / Api), CQRS via MediatR
- **Auth:** ASP.NET Core Identity + OpenIddict (OAuth2/OIDC authorization code + PKCE flow)
- **Frontend:** Angular 20 (admin UI), angular-oauth2-oidc
- **Data:** PostgreSQL, Redis
- **Messaging:** Kafka via Redpanda (transactional outbox + fan-out consumers)
- **AI:** Azure OpenAI + pgvector (from Phase 5)
- **Infra:** Docker Compose (local), Kubernetes/AKS (from Phase 7)

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

## Build roadmap

Each phase is a working, demoable increment.

- [x] **Phase 0 — Foundation.** Clean Architecture solution, Angular admin shell, Docker Compose, CI.
- [x] **Phase 1 — Core CMS.** Content types/blocks/pages, draft→publish workflow with versioning, REST content delivery API.
- [x] **Phase 2 — Auth & Authorization.** OIDC login via OpenIddict (authorization code + PKCE), access/refresh token rotation, RBAC → resource-based ownership policy, account lockout + rate limiting, audit log. (MFA and multi-tenant claims deferred — not needed yet with a single admin team.)
- [x] **Phase 3 — Event-driven backbone.** Kafka (Redpanda) with transactional outbox; PagePublished domain event drives three independent consumers - Redis cache invalidation, search re-indexing, and audit trail - decoupled from the write path entirely.
- [x] **Phase 4 — DXP: personalization & experimentation.** PageVariant targeting by audience segment with priority ordering, stable per-visitor A/B traffic splitting (SHA256 bucketing), delivery-time selection via PersonalizationEngine, and an analytics dashboard (impressions/conversions/rate) fed by a fourth Kafka consumer aggregating VariantServed/ConversionRecorded events.
- [ ] **Phase 5 — AI-driven features.** AI content assistant in the editor, semantic search/recommendations via pgvector, AI auto-tagging on publish.
- [ ] **Phase 6 — Polish.** OpenTelemetry observability, architecture docs, live Azure deployment.
- [ ] **Phase 7 — Kubernetes.** Helm chart / manifests, ConfigMaps/Secrets, HPA on the API, deployed to AKS.
