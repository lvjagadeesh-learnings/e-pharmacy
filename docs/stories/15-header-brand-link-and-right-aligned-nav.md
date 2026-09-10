# User Story: 15 — Clickable brand logo and right-aligned header navigation

**Status:** done

**As an** e-Pharmacy user,
**I want** clicking the "e-Pharmacy" brand name to take me to the homepage, and all header navigation (Membership, Orders, Cart, Log in/Sign up, Log out) grouped on the right side,
**so that** the header feels like a familiar, predictable site layout (brand top-left, actions top-right).

## Acceptance Criteria

- [x] Clicking the "e-Pharmacy" heading/brand navigates to `/` (the catalog/homepage)
- [x] The brand ("e-Pharmacy" + tagline) is aligned to the left of the header
- [x] All navigation/action items (Membership, and depending on auth state: Orders/Cart/Log out, or Log in/Sign up) are grouped together and aligned to the right of the header
- [x] Layout remains usable/readable at common mobile widths (wraps sensibly, no overlap)

## Notes

- Purely a header layout/markup change — no new routes, no auth/cart logic changes.
- Depends on story 14 (header shows Log in/Sign up when signed out, Orders/Cart/Log out when signed in) already being in place.
- Split out from a combined request (brand-link + right-aligned nav + cart icon + username display); this story covers the layout/positioning half — see story 16 for the cart icon and signed-in username display.
