---
name: testing-strategy
description: Apply a layered testing strategy when planning tests, reviewing test coverage, designing CI gates, or deciding where confidence should be proven. Use the cheapest layer that can prove the behavior, keep cross-repo contracts explicit, and reserve E2E tests for real user journeys.
---

# Testing Strategy

Use this skill when adding tests, reviewing a test suite, planning CI stages, or deciding whether a behavior belongs in unit, integration, contract, or E2E coverage.

Inspect the repository before applying stack-specific examples; adapt tools and commands to the actual backend, frontend, and CI configuration.

## Core principles

- **Cheapest layer proves it.** Put business rules in domain tests, orchestration and authorization in application tests, serialization and HTTP contracts in API tests, cross-repository compatibility in contract tests, and real journeys in a small E2E suite.
- **Repos earn confidence in isolation.** Keep a thin contract layer between repositories and a small E2E suite for wiring. Avoid an ice-cream-cone test pyramid.
- **No E2E for logic.** If an E2E test is proving a business rule, add the missing lower-layer test instead.
- **No single global coverage number.** Set meaningful expectations by layer. Domain and application behavior deserve a high bar; DTOs, generated code, and framework configuration may need little or no direct coverage.

## Test pyramid

Use these proportions as a starting point, not a quota:

- **Unit — domain and application:** ~60%; fast and numerous.
- **Integration / API:** ~25%; meaningful infrastructure and vertical-slice paths.
- **Contract:** ~10%; compatibility at repository seams.
- **E2E:** ~5%; slow, costly, and focused on critical user journeys.

When the suite drifts toward many E2E tests and few lower-layer tests, move logic coverage downward rather than adding more browser scenarios.

## Backend layers

For a .NET DDD backend, use the following boundaries. For another backend stack, preserve the responsibilities and substitute the repository's established tools.

| Layer | Proves | Policy | Cadence |
| --- | --- | --- | --- |
| **Domain unit** | Aggregate invariants, value-object validation, entity transitions, domain events, and business rules | TDD-first; avoid infrastructure, mocks, and framework re-tests | Every PR |
| **Application / use-case** | Handler orchestration, validation, authorization, and event dispatch | Mock only ports; avoid database access and re-asserting domain rules | Every PR |
| **Integration** | ORM mapping, SQL translation, migrations, transactions, concurrency, and constraints | Use real or containerized infrastructure where it is the behavior under test | Every PR; heavier cases nightly |
| **API functional** | Routing, auth, binding, validation, handler, domain, database, serialized responses, and status codes | Cover representative happy and sad paths, including 401/403 where relevant | Every PR |

Cross-cutting checks may include architecture fitness, property-based tests, mutation testing, and snapshots. Apply them where they expose a real risk; do not add a tool only to increase a metric.

Coverage expectations:

- Domain: high line coverage and a strong mutation score where mutation testing is available.
- Application: high behavioral coverage.
- Integration and API functional: meaningful paths, not a vanity percentage.
- DTOs, generated code, mappers, and framework configuration: low or no direct target unless they contain behavior.

## Frontend layers

For a React frontend, preserve these responsibilities and adapt the tools to the repository's setup.

| Layer | Proves | Policy | Cadence |
| --- | --- | --- | --- |
| **Unit** | Pure functions, hooks, reducers, formatters, and selectors | Test behavior through public interfaces | Every PR |
| **Component** | User-visible behavior and interactions | Query by role, label, or visible text; avoid implementation details and blind snapshots | Every PR |
| **Integration** | Complete flows at the network boundary | Use MSW or the repository equivalent; keep handlers aligned with the published API contract | Every PR |

Coverage expectations:

- Hooks, reducers, and formatters: high behavioral coverage.
- Components: cover important loading, empty, error, and success states rather than chasing a percentage.
- Generated and boilerplate code: no direct target unless customized behavior exists.

## Cross-repository contracts

Treat the contract as the seam between independently developed repositories. Prefer consumer-driven contracts such as Pact, or generate and validate an OpenAPI contract shared by the API client and mocked handlers.

Contract tests should catch a renamed or removed backend field without requiring the entire stack to be running. Run consumer publication and provider verification on every PR when both repositories participate in the change.

## Security and performance

- **Performance:** exercise real money-path or otherwise critical endpoints against a deployed or ephemeral environment, not only in-process. Define explicit p95/p99 latency ceilings and a maximum error rate. Use smoke checks optionally on PRs, load/soak nightly, and stress/soak as a pre-release gate.
- **Security:** use the repository's configured static analysis, dependency scanning, dependency review, secret scanning, and push protection. A GHAS setup commonly includes CodeQL with an appropriate query suite, Dependabot, dependency review, and secret scanning.
- **Runtime security gap:** static analysis and dependency scanning do not test a running application. Add deployed-stack DAST such as OWASP ZAP when the risk profile requires runtime authorization, injection, header, or TLS checks. Make the decision explicit.
- **Accessibility and visual regression:** treat accessibility checks as a PR gate for user-facing frontends; run visual regression on previews and nightly where it provides value.

## Pipeline map

| Stage | Required focus |
| --- | --- |
| **Every PR** | Domain/application unit tests; relevant integration/API tests; frontend unit/component/network-boundary tests; contract verification; architecture checks; accessibility checks; security gates; optional performance smoke |
| **Merge to main** | A small full E2E suite against the integrated environment |
| **Nightly** | Mutation testing; heavier integration; full visual regression; load/soak; scheduled security scans; optional DAST |
| **Pre-release** | Full E2E; accessibility audit; thresholded stress/soak gate |

Only add a stage when the repository can run it reliably and the result changes a release decision.

## TDD placement

- **Test-first:** domain and application/use-case logic.
- **Test-alongside, outside-in:** integration, API functional, frontend component, and network-boundary tests.
- **Test-after:** E2E, performance, and visual regression, because these validate an already-established user journey or operational characteristic.

Use the `tdd` skill for red-green-refactor work and the `codebase-design` skill when the correct test seam or module interface is unclear.

## Trust policies

- **No ice-cream cone:** move logic coverage down when E2E coverage grows.
- **No global coverage gate:** set targets per layer and behavior.
- **Flaky-test quarantine:** quarantine a flaky test quickly, assign an owner, and fix or delete it within one working day; do not silently ignore recurring flakes.
- **Mocks only at ports:** application tests mock repository or service interfaces, never concrete infrastructure.
- **Keep mocks honest:** derive network mocks and contract expectations from the published API contract where possible.
- **Every test has a reason:** name the behavior, seam, and pipeline stage it protects before adding it.

## Applying the strategy

Before changing tests or CI:

1. Inspect the repository's architecture, existing test projects, scripts, CI workflows, and API contracts.
2. Identify the behavior and the cheapest layer that can prove it.
3. Check whether an existing test at that seam can be extended before creating a new layer or tool.
4. Define the test's observable outcome and its cadence: PR, merge, nightly, or pre-release.
5. Add only the smallest useful test or gate, then run the narrowest relevant command and the repository's required broader checks.
6. Record deliberate gaps, such as untested generated code or an intentionally omitted DAST stage, in the plan or review.

A testing strategy is working when the suite gives fast PR feedback, protects real behavior at the cheapest seam, and keeps slower tests focused on integration, compatibility, user journeys, and operational risk.
