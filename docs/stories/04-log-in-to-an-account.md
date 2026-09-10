# User Story: 04 — Log in to an existing account

**Status:** done

**As a** registered user,
**I want** to log in with my email and password,
**so that** I can access my account, cart, and order history.

## Acceptance Criteria

- [x] A login form accepts email and password
- [x] Invalid credentials show a clear error without revealing whether the email exists
- [x] Successful login establishes an authenticated session and redirects into the app
- [x] Protected areas (cart, orders, reviews, membership) are inaccessible while logged out

## Notes

- Depends on story 03 (sign-up) for accounts to exist.
- Session mechanism (cookie vs. token) is left to the implementation plan.
