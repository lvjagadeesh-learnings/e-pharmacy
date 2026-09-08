---
title: Repository instructions — e-Pharmacy (Gen-e2)
---

# e-Pharmacy — repository instructions

This repository is developed exclusively using the **Gen-e2 delivery workflow** (`.github/skills`, `.github/agents`): discover → decide → plan → implement → validate → release. Before planning or implementing, consult the matching skill in `.github/skills/` and the matching agent in `.github/agents/`.

## Layout

Monorepo:

- `frontend/` — React 19 + Vite, JavaScript (no TypeScript)
- `backend/` — .NET 10, Clean/DDD layered solution
- `docs/stories/` — user stories and technical enabler stories (`generate-stories` skill)
- `docs/implementation-plans/` — plans produced by the `implementation-plan` skill and executed by `execute-plan`
- `docs/definition-of-done.md` — the quality gate every task must pass
- `.github/skills/`, `.github/agents/` — the reusable Gen-e2 workflow (do not treat as product code)

## Frontend stack

- React 19, Vite, **JavaScript only** — do not introduce TypeScript
- Package manager: npm
- Tests: Vitest + React Testing Library (`@testing-library/jest-dom`), query by role/label/text — no snapshot-only tests, no testing implementation details
- Lint: ESLint (Vite's default React config)

## Backend stack

- .NET 10, Clean/DDD layered solution: `EPharmacy.Domain` (no dependencies) → `EPharmacy.Application` (use cases, ports) → `EPharmacy.Infrastructure` (EF Core, repositories) → `EPharmacy.Api` (ASP.NET Core hosting, DI wiring)
- Persistence: EF Core with SQLite for local/dev. **SQLite is a dev-only choice** — revisit before any production deployment.
- Tests: xUnit + FluentAssertions + Moq. Domain and Application tests mock only ports (no infrastructure). API tests use `WebApplicationFactory`.

## Testing strategy

Follow the `testing-strategy` skill's layered pyramid: domain/application unit tests test-first (TDD), integration/API functional tests test-alongside, E2E test-after and reserved for real user journeys. No business logic proven only by an E2E test.

## Definition of Done

See `docs/definition-of-done.md`. A task is not complete until every criterion there passes.

## Conventions

- Prefer the specialist agent that matches the task (`discovery-agent`, `decision-agent`, `planning-agent`, `delivery-agent`, `quality-agent`, `release-agent`).
- Keep changes surgical; validate before release; do not push or open a PR without explicit confirmation.
