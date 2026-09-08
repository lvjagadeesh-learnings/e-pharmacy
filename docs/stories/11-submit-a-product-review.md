# User Story: 11 — Submit a review for a purchased medicine

**Status:** done

**As a** user who has received an order,
**I want** to submit a review (rating and comment) for a medicine I bought,
**so that** I can share feedback and help other shoppers decide.

## Acceptance Criteria

- [x] A review form (rating + text) is available for medicines the user has an order for
- [x] Submitted reviews are persisted and associated with the user and the medicine
- [x] Reviews are visible on the medicine's catalog/detail view
- [x] Basic validation prevents empty submissions

## Notes

- Whether a review requires a "received" order (story 10) or any order at all is an open question — default assumption is the order must reach a received/delivered status first.
- Editing/deleting a submitted review was not requested — out of scope unless asked for later.
