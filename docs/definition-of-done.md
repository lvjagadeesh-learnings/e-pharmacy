# Definition of Done

A task from a Gen-e2 implementation plan is **done** only when every applicable item below passes.

## All tasks

- [ ] Code builds without errors
- [ ] No lint/format violations
- [ ] Acceptance criteria mapped to this task are verified
- [ ] Plan file's Task Breakdown and File Changes checkboxes updated
- [ ] Story status updated (`ready` → `in progress` → `done`) when applicable

## Frontend tasks (`frontend/`)

- [ ] `npm run build` succeeds
- [ ] `npm run lint` passes
- [ ] `npm run test` (Vitest) passes, including any tests added/changed by this task
- [ ] New behavior has component/unit test coverage per the `testing-strategy` skill (loading, empty, error, success states as relevant)

## Backend tasks (`backend/`)

- [ ] `dotnet build` succeeds for the whole solution
- [ ] `dotnet test` passes for all test projects (`Domain.Tests`, `Application.Tests`, `Api.Tests`)
- [ ] Domain/Application behavior added or changed test-first (red → green)
- [ ] EF Core migrations included when the model changes, and apply cleanly to a fresh SQLite database

## Before release (final task of a plan)

- [ ] Full Definition of Done re-run across the whole plan (not just the last task)
- [ ] `node scripts/validate-repository.mjs` still passes (Gen-e2 scaffold structure intact)
- [ ] Deviations from the plan recorded in the plan's `## Deviations` table, if any
