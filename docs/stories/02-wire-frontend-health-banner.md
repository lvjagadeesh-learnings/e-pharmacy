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
