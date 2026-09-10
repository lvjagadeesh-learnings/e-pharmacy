# User Story: 12 — View and join a membership tier

**Status:** ready

**As a** registered user,
**I want** to see membership tier/benefits and join or view my membership status,
**so that** I can get the value (discounts/perks) of being a member.

## Acceptance Criteria

- [ ] A membership page explains the single "e-Pharmacy Plus" tier and its benefit (10% off every order), and is visible to anyone (logged out or in)
- [ ] A logged-in user who is not yet a member sees a "Join" form (simulated payment, same convention as checkout) and, on success, becomes a member immediately
- [ ] A logged-in member sees their membership status (member since date) instead of the join form
- [ ] Once a user is a member, their order totals at checkout reflect the 10% discount

## Notes

- Follow-up scoping conversation held with the user; resolved to a single-tier MVP:
  - **Tier**: one tier only, "e-Pharmacy Plus" (no comparison matrix).
  - **Benefit**: 10% discount applied to order totals at checkout.
  - **Enrollment cost**: paid, via the existing simulated `FakePaymentGateway` (same "always declines" test-card convention as checkout) — framed as a one-time simulated charge representing the membership fee; no real recurring billing exists in this app.
- Status moved from `draft` to `ready` now that the ambiguity called out previously is resolved.
