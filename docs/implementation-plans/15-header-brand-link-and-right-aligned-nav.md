# Implementation Plan: 15 — Header brand link and right-aligned navigation

## Story

**As an** e-Pharmacy user,
**I want** the "e-Pharmacy" brand to link home and all nav actions grouped on the right,
**so that** the header follows a familiar left-brand/right-actions layout.

Acceptance criteria: see `docs/stories/15-header-brand-link-and-right-aligned-nav.md`.

## Context

- `AppHeader()` in `frontend/src/App.jsx` currently renders `<h1>e-Pharmacy</h1>` as plain text (no link) followed by the tagline `<p>`, then all nav links/buttons inline in the same flex row — nothing is grouped or aligned right.
- `#app-header` (in `App.css`) is already `display: flex; flex-wrap: wrap; align-items: center; gap: var(--space-4)` — a right-aligned action group is a small, additive CSS change (a wrapping `<div>`/`<nav>` with `margin-left: auto`), not a rewrite.

## Approach

Wrap the brand (`h1` + tagline `p`) in a `.header-brand` container, and wrap everything else (Membership/Orders/Cart/Log in/Sign up/Log out) in a `.header-actions` container. Make the `<h1>` content a `<Link to="/">` instead of plain text. In CSS, give `.header-actions` `margin-left: auto` (pushes it to the far right within the flex row) and keep `flex-wrap: wrap` so it drops to its own line on narrow viewports instead of overflowing.

## Architecture

```
frontend/src/
├─ App.jsx    modified — AppHeader(): brand wrapped in Link + .header-brand; actions wrapped in .header-actions
└─ App.css    modified — .header-brand, .header-actions (margin-left: auto) rules
```

## File Changes

- [x] `frontend/src/App.jsx` — `AppHeader()`: `<h1><Link to="/">e-Pharmacy</Link></h1>` inside `.header-brand`; all nav items inside `.header-actions`
- [x] `frontend/src/App.css` — add `.header-brand`, `.header-actions { margin-left: auto; display: flex; flex-wrap: wrap; align-items: center; gap: var(--space-4); }`
- [x] `frontend/src/App.test.jsx` — assert the brand heading is a link to `/`

## Task Breakdown

1. [x] Update `AppHeader()` JSX: wrap brand in `.header-brand` with `<h1>` containing a `<Link to="/">e-Pharmacy</Link>`; wrap the `Membership`/auth-state block in `.header-actions`.
2. [x] Add `.header-brand`/`.header-actions` CSS rules in `App.css` (right-align actions, preserve wrap behavior on narrow widths).
3. [x] Extend `App.test.jsx`: assert `screen.getByRole('heading', { name: 'e-Pharmacy' })` contains a link with `href="/"`.
4. [x] Run frontend test suite (51/51 passing); manual browser check pending alongside story 16.
5. [x] `node scripts/validate-repository.mjs`, commit.
