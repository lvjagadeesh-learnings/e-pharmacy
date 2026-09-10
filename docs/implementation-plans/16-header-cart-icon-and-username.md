# Implementation Plan: 16 — Header cart icon and signed-in username

## Story

**As an** e-Pharmacy user,
**I want** a cart icon (with an item-count badge) instead of the word "Cart", and my name shown once signed in,
**so that** the cart is recognizable at a glance and I know which account is active.

Acceptance criteria: see `docs/stories/16-header-cart-icon-and-username.md`.

## Context

- Builds on story 15's `.header-actions` right-aligned group in `AppHeader()` (`frontend/src/App.jsx`).
- `useCart()` already exposes `itemCount`; `useAuth()` already exposes `user` with a `displayName` field (used today by `AccountPage`).
- No icon library/dependency exists in `frontend/package.json` — a small inline SVG component avoids adding one for a single icon.

## Approach

Add a tiny `CartIcon` SVG component (outline shopping-cart glyph, `aria-hidden="true"` since the parent link carries the accessible name) rendered inside the existing `Link to="/cart"`. Replace the visible "Cart (N)" text with the icon plus a small `.cart-count` badge `<span>` shown only when `itemCount > 0`; give the `<Link>` an `aria-label={`Cart, ${itemCount} item(s)`}` so it stays accessible without visible text. Add a `.header-username` `<span>` rendering `user.displayName` inside `.header-actions`, only rendered in the already-existing `user`-truthy branch (right next to `Log out`).

## Architecture

```
frontend/src/
├─ App.jsx    modified — CartIcon SVG; cart Link uses icon+badge+aria-label; header-username span
└─ App.css    modified — .cart-link/.cart-count badge styling, .header-username styling
```

## File Changes

- [x] `frontend/src/App.jsx` — add `CartIcon()` component; update cart `Link` (icon + conditional count badge + `aria-label`); add `.header-username` span showing `user.displayName`
- [x] `frontend/src/App.css` — `.cart-count` badge (small circle, positioned on the icon), `.header-username` text styling
- [x] `frontend/src/App.test.jsx` — update cart assertions to query by accessible name/label instead of visible "Cart (N)" text; add a test asserting the display name renders when signed in and not when signed out

## Task Breakdown

1. [x] Add an inline `CartIcon` SVG component in `App.jsx` (`aria-hidden="true"`, currentColor stroke so it inherits header text color).
2. [x] Replace the cart `Link`'s "Cart ({itemCount})" text with `<CartIcon /><span className="cart-count">{itemCount}</span>` (badge only rendered when `itemCount > 0`); add `aria-label` on the `Link` for accessibility.
3. [x] Add `<span className="header-username">{user.displayName}</span>` inside the signed-in branch of `.header-actions`, before the `Log out` button.
4. [x] Add `.cart-link`/`.cart-count`/`.header-username` CSS rules in `App.css`.
5. [x] Update `App.test.jsx`: replace `getByRole('link', { name: 'Cart (1)' })`-style assertions with `getByRole('link', { name: /Cart, \d+ item/i })`; add a test that the signed-in display name text appears, and one that it's absent when signed out.
6. [x] Run frontend test suite (55/55 passing); manual browser check confirmed (badge shows item count; "Demo User" shown when signed in).
7. [x] `node scripts/validate-repository.mjs`, commit.
