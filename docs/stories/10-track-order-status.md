# User Story: 10 — Track order status from placed to received

**Status:** ready

**As a** user who has placed an order,
**I want** to see my order's status update from placed through to delivered/received,
**so that** I know where my order is and when to expect it.

## Acceptance Criteria

- [ ] An order detail/tracking view shows the current status (e.g. Placed → Processing → Shipped → Delivered)
- [ ] The status history is visible to the user who placed the order
- [ ] The final "received"/"delivered" status is reachable and clearly distinguished from in-progress statuses

## Notes

- Real carrier tracking integration is out of scope; status progression is expected to be simulated/backend-driven, matching the "fake payment" precedent.
- Whether the user manually confirms receipt or the system marks it delivered automatically is an open question for the implementation plan.
- Depends on story 09 (an order must exist).
