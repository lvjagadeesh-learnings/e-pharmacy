# User Story: 16 — Cart icon and signed-in username in the header

**Status:** done

**As an** e-Pharmacy user,
**I want** the cart shown as a recognizable cart icon (with an item-count badge) instead of the word "Cart", and my name visible in the header once I'm signed in,
**so that** the cart is instantly recognizable and I can see at a glance which account I'm signed in as.

## Acceptance Criteria

- [x] The header's cart link is a cart icon (not the text "Cart"), with an accessible label (e.g. `aria-label="Cart, N items"`) so screen readers still announce it clearly
- [x] When the cart has one or more items, a small count badge is visible on/next to the icon; when empty, no distracting badge is shown
- [x] When signed in, the signed-in user's display name is visible in the header (top-right area, alongside the other nav actions)
- [x] When signed out, no username/placeholder is shown

## Notes

- Depends on story 15 (right-aligned header nav) for placement; this story covers the cart-icon and username-display content changes within that same right-aligned group.
- No icon library is added — a small inline SVG is used to avoid a new dependency for one icon, consistent with the rest of the frontend (plain CSS, no component library).
- Username source: `useAuth().user.displayName` (already returned by `/api/auth/me`, already used by `AccountPage`).
