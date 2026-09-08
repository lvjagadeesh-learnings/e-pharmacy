# User Story: 01 — Scaffold full-stack skeleton

**Status:** done

**As** the delivery team,
**I want** a working React 19/Vite frontend and a .NET 10 Clean/DDD backend skeleton with SQLite wired up and tested,
**so that** upcoming e-pharmacy feature stories have a real, conventions-following codebase to plan and implement against.

## Acceptance Criteria

- [x] `frontend/` is a Vite + React 19 app (JavaScript, not TypeScript) that builds (`npm run build`), runs a dev server, lints clean, and has a passing example component test (Vitest + React Testing Library)
- [x] `backend/` is a .NET 10 solution with `EPharmacy.Domain`, `EPharmacy.Application`, `EPharmacy.Infrastructure`, `EPharmacy.Api` projects following Clean/DDD dependency direction (Domain has no dependencies)
- [x] `backend/` uses EF Core with SQLite (dev-only) with at least one initial migration that applies cleanly to a fresh database
- [x] `EPharmacy.Api` exposes a `GET /health` endpoint returning 200
- [x] Each backend layer has its own xUnit test project (`Domain.Tests`, `Application.Tests`, `Api.Tests`) with at least one passing test proving the layer is wired correctly
- [x] `.github/copilot-instructions.md` and `docs/definition-of-done.md` exist and describe the stack/conventions above
- [x] `node scripts/validate-repository.mjs` (existing Gen-e2 scaffold check) still passes after the new folders are added

## Notes

- This is a technical enabler story, not an end-user INVEST slice — it exists to give the team a real codebase before feature stories (catalog, cart, prescriptions, orders, etc.) are planned.
- SQLite is explicitly a dev/local choice; production persistence is an open decision for a later story.
- No product feature brief exists yet for the e-pharmacy domain itself — that is expected to follow this story via `generate-stories`.
