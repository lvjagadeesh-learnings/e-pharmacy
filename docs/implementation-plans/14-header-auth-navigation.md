# Implementation Plan: 14 — Header Log in / Sign up / Log out navigation

## Story

**As an** e-Pharmacy user,
**I want** to see "Log in"/"Sign up" links when signed out and "Log out" when signed in,
**so that** account actions are always discoverable from the header.

Acceptance criteria: see `docs/stories/14-header-auth-navigation.md`.

## Context

- `AppHeader()` in `frontend/src/App.jsx` already conditionally renders `Orders`/`Cart`/`Log out` when `user` is truthy (from `useAuth()`), but never rendered anything for the signed-out case besides the always-visible `Membership` link — there was no `/login`/`/signup` link anywhere in the header.
- `LoginPage`/`SignUpPage`/logout (`useAuth().logout`) already exist and are fully implemented/tested (stories 03/04/05) — this is a header-only, additive change.
- Existing tests in `App.test.jsx` assert `Log out` button presence/absence by auth state; new tests follow the same pattern for `Log in`/`Sign up` links.

## Approach

Add an `else` branch (rendered when `!user`) in `AppHeader()` with two `<Link>`s: `Log in` → `/login` and `Sign up` → `/signup`. Style them with the existing `#app-header a` rule already used for `Membership`/`Orders`/`Cart` (no new CSS needed — the rule is generic). No changes to `LoginPage`, `SignUpPage`, auth context, or backend.

## Architecture

```
frontend/src/
├─ App.jsx        modified — AppHeader(): render Log in/Sign up links when !user
└─ App.test.jsx   modified — add signed-out-shows-links / signed-in-hides-links tests
```

## File Changes

- [x] `frontend/src/App.jsx` — `AppHeader()`: add signed-out branch with `Log in`/`Sign up` links
- [x] `frontend/src/App.test.jsx` — assert `Log in`/`Sign up` links visible when signed out and absent when signed in

## Task Breakdown

1. [x] Update `AppHeader()` in `App.jsx`: wrap the existing `{user && (...)}` block's sibling with `{!user && (<><Link to="/login">Log in</Link><Link to="/signup">Sign up</Link></>)}`.
2. [x] Extend `App.test.jsx`: signed-out render shows `Log in`/`Sign up` links and hides `Orders`/`Cart`/`Log out`; signed-in render shows `Orders`/`Cart`/`Log out` and hides `Log in`/`Sign up`.
3. [x] Run frontend test suite (`npm test` / `npx vitest run`) — confirm all pass (51/51).
4. [x] Manual browser verification: signed-out header shows Membership/Log in/Sign up; sign in; header switches to Membership/Orders/Cart/Log out.
5. [x] `node scripts/validate-repository.mjs`, commit.
