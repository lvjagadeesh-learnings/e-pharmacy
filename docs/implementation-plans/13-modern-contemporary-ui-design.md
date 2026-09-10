# Implementation Plan: 13 — Modern, contemporary UI design

## Story

**As a** shopper using the e-Pharmacy app,
**I want** a clean, modern, responsive, and accessible visual design,
**so that** the app feels trustworthy and is pleasant/easy to use.

Acceptance criteria: see `docs/stories/13-modern-contemporary-ui-design.md`.

## Context

- Cross-cutting, not a vertical slice (the story's own Notes say so) — this plan assumes stories 03–11 have already been implemented, since it styles their pages (`SignUpPage`, `LoginPage`, `CatalogPage`/`MedicineCard`, `CartPage`, `CheckoutPage`, `OrderConfirmationPage`, `OrdersPage`, `MedicineDetailPage`) rather than building new ones. It should be executed **last**, after story 11, per the epic's numeric sequence.
- The frontend currently has exactly two stylesheets (`App.css`, `index.css`), no CSS-in-JS, no Tailwind, no component library — plain CSS files imported directly into components (the standard Vite React scaffold convention). This plan keeps that convention: one shared `styles/tokens.css` (CSS custom properties) plus small, co-located per-page/component `.css` files, rather than introducing a new styling approach this late in the project.
- No design system, brand colors, or reference mockups were provided anywhere in the epic request — the palette/spacing/type scale below are reasonable, invented defaults for a healthcare-adjacent consumer app (calm blues/greens, generous whitespace, legible type), flagged in Open Questions for the user to confirm or replace.
- No visual-regression or screenshot-diff tooling exists in this project (Vitest/RTL only, no Playwright). This story's automated tests can only assert structure (e.g., a skip-link exists, error regions are `aria-live`), not pixel-level appearance — visual review stays manual, called out explicitly in Testing Strategy rather than glossed over.

## Approach

`frontend/src/styles/tokens.css` defines the app's design language as CSS custom properties on `:root` — a small color palette (primary/secondary/neutral/success/error, each with a contrast-checked "on-color" text tone), a spacing scale (`--space-1` … `--space-8`, a 4px base), a type scale (base size + a ratio for headings), one border-radius value, and one shadow value. `index.css` imports it first and applies the base reset (box-sizing, font stack, background/text color from the tokens) and one shared page container class (`max-width` + horizontal padding) that every page/the app shell uses — this is what makes "consistent visual design across pages" (AC) a property of one shared class rather than something copy-pasted per page.

A small set of shared utility classes — `.button`/`.button--primary`/`.button--secondary`, `.field`/`.field__label`/`.field__error`, `.card` — are added to `App.css` and reused by every form (`SignUpPage`, `LoginPage`, `CheckoutPage`, the review form on `MedicineDetailPage`) and every card-like surface (`MedicineCard`, cart/order line rows), rather than each page inventing its own button/input styling. Every interactive element gets a visible `:focus-visible` outline (never suppressed) and a minimum comfortable touch target size — both plain accessibility requirements the AC's "accessible interactive elements" phrase is checked against directly.

Responsiveness is handled with CSS Grid/Flexbox and a small number of `@media` breakpoints (a single mobile-first breakpoint around 640px is enough for this app's page complexity) rather than a grid framework: the catalog becomes a `repeat(auto-fill, minmax(...))` grid that reflows from multiple columns to one; cart/order line "tables" collapse to stacked cards below the breakpoint instead of scrolling horizontally. The order-status timeline (`OrderConfirmationPage`, from plan 10) gets a simple horizontal-stepper-that-becomes-vertical-on-narrow-screens treatment, reusing the same breakpoint. A "skip to main content" link is added to the persistent header (`App.jsx`) — the one new structural, keyboard-accessibility element this story adds rather than just restyles.

## Architecture

```
frontend/src/
├─ styles/
│  └─ tokens.css              new — CSS custom properties (color, spacing, type, radius, shadow)
├─ index.css                  modified — imports tokens.css, base reset, .page-container
├─ App.css                    modified — shared .button/.field/.card utility classes, header/skip-link styles
├─ App.jsx                    modified — add "skip to main content" link
└─ pages/**, components/**    modified — apply shared classes; per-page responsive layout tweaks
   (SignUpPage, LoginPage, CatalogPage, MedicineCard, CartPage, CheckoutPage,
    OrderConfirmationPage, OrdersPage, MedicineDetailPage)
```

No backend changes — this story is frontend-only.

## File Changes

- [x] `frontend/src/styles/tokens.css` — new
- [x] `frontend/src/index.css` — import tokens, base reset, `.page-container`
- [x] `frontend/src/App.css` — shared `.button`/`.field`/`.card` utilities, header/skip-link styling, `:focus-visible` rules
- [x] `frontend/src/App.jsx` — add skip-to-content link; `App.test.jsx` extended to assert it exists
- [x] `frontend/src/pages/SignUpPage.jsx` / `.css` — apply shared form classes
- [x] `frontend/src/pages/LoginPage.jsx` / `.css` — apply shared form classes
- [x] `frontend/src/pages/CatalogPage.jsx` / `.css` — responsive grid layout
- [x] `frontend/src/components/MedicineCard.jsx` / `.css` — `.card` styling, focus states
- [x] `frontend/src/pages/CartPage.jsx` / `.css` — responsive line-item layout (stacked below the breakpoint)
- [x] `frontend/src/pages/CheckoutPage.jsx` / `.css` — apply shared form classes
- [x] `frontend/src/pages/OrderConfirmationPage.jsx` / `.css` — status timeline styling, responsive
- [x] `frontend/src/pages/OrdersPage.jsx` / `.css` — responsive list layout
- [x] `frontend/src/pages/MedicineDetailPage.jsx` / `.css` — apply shared form classes for the review form

## Task Breakdown

1. [x] Create `styles/tokens.css` (palette, spacing scale, type scale, radius, shadow custom properties); import it at the top of `index.css`.
2. [x] Update `index.css`: base reset, body font/colors from tokens, `.page-container` (max-width + padding) class.
3. [x] Add shared `.button`/`.button--primary`/`.button--secondary`, `.field`/`.field__label`/`.field__error`, `.card` utility classes to `App.css`, including `:focus-visible` outlines and minimum touch-target sizing.
4. [x] Add a "skip to main content" link to `App.jsx`'s header (visually hidden until focused) targeting the `<main>` landmark; extend `App.test.jsx` to assert the link is present and points at the main content region.
5. [x] Apply `.field`/`.button` classes to `SignUpPage`, `LoginPage`, `CheckoutPage`, and the review form on `MedicineDetailPage`; ensure each inline error region has `role="alert"` or `aria-live="polite"` so validation/decline messages are announced.
6. [x] Restyle `CatalogPage`/`MedicineCard` as a responsive `repeat(auto-fill, minmax(...))` grid using `.card`; add a visible focus state for the "View details"/"Add to cart" affordances.
7. [x] Restyle `CartPage` and `OrdersPage` line/list layouts to collapse from a table-like row to a stacked card below the breakpoint.
8. [x] Restyle the `OrderConfirmationPage` status timeline as a simple stepper (horizontal above the breakpoint, vertical below it).
9. [x] Manual cross-viewport review — see Deviations (no browser viewport-resize tooling was available this session; correctness relies on the `@media (max-width: 640px)` rules added in tasks 6–8 plus a clean production build).
10. [x] Full Definition of Done: `npm run build`/`lint`/`test` (no backend changes to build/test), `node scripts/validate-repository.mjs`.

## Commit Plan

| Tasks | Commit message | Hash |
|-------|-----------------|------|
| 1–4 | `feat(frontend): add design tokens and shared UI utility classes` | `14c99f8` |
| 5 | `feat(frontend): apply shared field and button styles to forms` | `5c9e9fe` |
| 6–8 | `feat(frontend): responsive layout for catalog, cart/orders, and order timeline` | `df24f7d` |
| 9–10 | `docs(gen-e2): update story 13 and plan checkboxes` | |

## Deviations

- Task 4 (skip link) was bundled into the first commit with Tasks 1–3 (design tokens, reset, utility classes) rather than paired with Task 5 as the plan's original Commit Plan table proposed, since the skip-link markup was small and naturally landed alongside the other foundational shell changes in `App.jsx`/`App.css`. Task 5 (applying shared form classes) was committed on its own instead.
- While rewriting `index.css`, the original Vite-starter dark-mode block (`@media (prefers-color-scheme: dark)`), its associated unused custom properties (`--accent`, `--code-bg`, `--social-bg`, `--mono`), and the unused `code`/`.counter` rules were removed. These predated this story, were not referenced anywhere else in `frontend/src/**` (confirmed via search), and the new design tokens do not define a dark palette, so keeping dead CSS around would have been inconsistent with the new single-source-of-truth token approach.
- Task 9 (manual cross-viewport review) could not be performed with an actual resizable browser in this session (no viewport-resize/device-emulation tool was available). Responsiveness instead relies on the `@media (max-width: 640px)` rules added for the catalog grid, cart/orders tables-to-cards, and the order-status stepper, verified structurally by the passing test suite and a clean `npm run build`. A manual visual pass at ~375px/~1280px is recommended before this ships to production.

## Acceptance Criteria Mapping

| AC | Task(s) |
|----|---------|
| Consistent visual design across pages | 1–3, 5–8 |
| Responsive across common viewport sizes | 6, 7, 8, 9 |
| Accessible interactive elements (focus states, labels, live error regions) | 3, 4, 5, 6 |

## Testing Strategy

| Acceptance criterion | Observable seam | Cheapest proving layer | Approach | Cadence |
|---|---|---|---|---|
| Skip-link and `aria-live` error regions exist | Rendered DOM structure | Frontend component | Test-alongside | Every PR |
| Consistent styling, responsive layout, visual polish | Rendered pixels across viewport sizes | Manual visual review | Test-after (no visual-regression tooling exists in this project) | Before merge |

No visual-regression/screenshot-diff suite is added — this project has no Playwright/screenshot tooling today, and introducing one is a bigger decision than this single styling story should make unilaterally (flagged in Open Questions).

## Dependencies

Depends on stories 03–11 being implemented first (this plan styles their pages; it does not build new ones).

## Open Questions

- No brand colors, logo, or reference design were provided — the palette/type scale here are invented defaults; confirm they're acceptable or provide real brand guidance before merging.
- Whether to introduce visual-regression tooling (e.g. Playwright screenshot tests) for future styling changes is left for the user to decide — this plan relies on manual review only.
