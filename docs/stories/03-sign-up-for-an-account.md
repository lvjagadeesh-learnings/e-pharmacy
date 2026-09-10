# User Story: 03 — Sign up for a new account

**Status:** done

**As a** new visitor to the e-Pharmacy app,
**I want** to create an account with my email and a password,
**so that** I can place orders and manage them under my own account.

## Acceptance Criteria

- [x] A sign-up form collects at minimum name, email, and password
- [x] Duplicate email registration is rejected with a clear error message
- [x] Passwords are hashed server-side and never stored or transmitted in plain text
- [x] On successful sign-up, the user is authenticated and redirected into the app
- [x] Client- and server-side validation covers required fields and basic email format

## Notes

- First story in the account/auth slice; nothing else depends on it, everything account-related depends on it.
- Password strength rules and email verification are not specified by the request — open questions to confirm before planning.
