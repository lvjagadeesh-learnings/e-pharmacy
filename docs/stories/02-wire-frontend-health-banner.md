# User Story: 02 — Wire frontend shell to display e-Pharmacy health status

**Status:** done

**As** a user opening the e-Pharmacy app,
**I want** the frontend to show real e-Pharmacy content instead of the default Vite/React starter template,
**so that** the running app reflects the actual product instead of the scaffold it was generated from.

## Acceptance Criteria

- [x] `frontend/src/App.jsx` no longer renders the default Vite/React starter markup (hero image, counter button, docs/social links)
- [x] `App.jsx` renders the existing `HealthBanner` component, wired to call the backend's `GET /health` endpoint and display its status
- [x] Loading, error, and success states are handled and covered by component tests (Vitest + React Testing Library), per the `testing-strategy` skill
- [x] `npm run build`, `npm run lint`, and `npm run test` all pass
- [x] Full Definition of Done re-run

## Notes

- Root cause: story 01 scaffolded the frontend/backend skeleton and created `HealthBanner.jsx`, but never rendered it from `App.jsx` — the default Vite template was left in place. That is why the browser at `http://localhost:5173/` shows the Vite starter UI instead of e-Pharmacy content.
- This is a technical enabler story; no e-Pharmacy product feature brief exists yet, so this only wires the shell to the health endpoint — not real domain UI (catalog, cart, prescriptions, etc.).
- **Superseded (post-story-13):** `HealthBanner` and its wiring into `App.jsx` were removed as part of story 13's contemporary UI redesign of the persistent app shell (header/nav). Once the app had real product pages (catalog, cart, checkout, orders, membership) and its own polished chrome, a raw backend-health banner was no longer appropriate as permanent, user-facing UI — it was scaffolding for the pre-story-13 skeleton, not a product requirement. The `/health` endpoint itself, `RecordHealthCheckHandler`, and their backend tests are untouched; only the frontend banner and its component/hook were removed, and `App.test.jsx` was updated accordingly. This removal was not documented at the time it happened; this note retroactively records it so the acceptance criteria above are understood as historical (met at the time, then intentionally superseded) rather than a live regression.
