# User Story: 14 — See Log in / Sign up / Log out in the header

**Status:** done

**As an** e-Pharmacy user,
**I want** to see "Log in" and "Sign up" links in the header when I'm signed out, and a "Log out" option when I'm signed in,
**so that** I can always find the right account action without knowing the `/login` or `/signup` URLs by heart.

## Acceptance Criteria

- [x] When signed out, the header shows a "Log in" link and a "Sign up" link, alongside the always-visible "Membership" link
- [x] When signed out, the header does **not** show "Orders", "Cart", or "Log out" (auth-only links stay hidden until signed in)
- [x] When signed in, the header shows "Orders", "Cart", and "Log out", alongside "Membership", and does **not** show "Log in"/"Sign up"
- [x] Clicking "Log out" ends the session and returns the header to the signed-out state (existing behavior, unchanged)

## Notes

- Login (story 04), sign-up (story 03), and logout (story 05) already work end-to-end — the pages and API calls exist and are tested. The gap is purely navigational: the header (`App.jsx`) never rendered links to `/login` or `/signup`, so a signed-out user had no visible way to discover those pages. This story only adds/adjusts header links; no new pages, endpoints, or auth logic.
