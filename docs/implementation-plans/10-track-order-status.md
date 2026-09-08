# Implementation Plan: 10 — Track order status through to delivery

## Story

**As a** shopper who has placed an order,
**I want** to see its status progress from placed to delivered,
**so that** I know where my order is.

Acceptance criteria: see `docs/stories/10-track-order-status.md`.

## Context

- Extends plan 09's `Order` aggregate and its `GET /api/orders/{orderId}` endpoint — per plan 09's own note, this story adds a status field/history to that *same* response and page rather than introducing a parallel one.
- Real carrier tracking is out of scope (explicitly simulated, per the story's Notes) — status must progress on its own without any external integration or background job, so it's computed deterministically from elapsed time since `PlacedAtUtc` rather than stored/mutated by a scheduler.
- The story's Notes flag "manual vs. automatic receipt confirmation" as an open question. This plan resolves it: **manual** — the shopper clicks "Mark as received" once the computed status reaches `Delivered`. This mirrors how real e-commerce/marketplace "confirm receipt" flows work, is trivial to implement and test (one more piece of `Order` state, no scheduler), and is more meaningful than an automatic timer silently flipping to "received."
- Nothing today lets a shopper reach any order after leaving the confirmation page — there's no orders list. This plan adds a minimal `GET /api/orders` (current user's orders) and `/orders` page purely so the feature the story asks for ("view status of my orders") is actually reachable, not as a scope expansion.

## Approach

`OrderStatus` (Domain enum: `Placed`, `Processing`, `Shipped`, `Delivered`) is never stored as a column that something else has to remember to update — instead, a pure `OrderStatusCalculator` (Domain) computes it from `order.PlacedAtUtc` and the current time using fixed offsets (`Processing` at +30s, `Shipped` at +90s, `Delivered` at +180s from placement — short enough to demo the full progression within a few minutes, long enough that "placed" is visibly its own state right after checkout). The same calculator produces a `statusHistory` — one entry per status already reached, each with its (computed, not stored) timestamp — satisfying "status history visible" without a status-change event log. Because it's a pure function of two `DateTimeOffset`s, it's fully unit-testable without touching the clock, a database, or a background job.

`Order` gains one more piece of *real* state beyond what plan 09 gave it: a nullable `ReceivedAtUtc` and a `MarkReceived(nowUtc)` method that throws unless the calculator says the order has reached `Delivered` — you can't confirm receipt of something not yet delivered. `GetOrderHandler` (extended from plan 09's inline order-fetch logic, now its own handler) returns the computed `status`, `statusHistory`, and `receivedAtUtc` alongside the existing fields. `POST /api/orders/{orderId}/receive` (new, `.RequireAuthorization()`, ownership-checked like `GET /api/orders/{orderId}` already is) calls `MarkReceived`; `409 Conflict` if the order isn't `Delivered` yet or was already marked received.

`GET /api/orders` (list, current user only, ordered newest-first) returns each order's id, reference number, placed-at, computed status, and total — enough for an `OrdersPage.jsx` (`/orders`, `RequireAuth`) list linking into the (now status-aware) `/orders/:orderId` page. `OrderConfirmationPage.jsx` (from plan 09) is extended in place to show the status timeline and, once `Delivered` and not yet received, a "Mark as received" button — it isn't renamed or replaced, since it's the same page doing more.

## Architecture

```mermaid
sequenceDiagram
    participant UI as OrderConfirmationPage.jsx
    participant Api as GET /api/orders/{orderId}
    participant App as GetOrderHandler
    participant Calc as OrderStatusCalculator
    participant Repo as IOrderRepository

    UI->>Api: fetch('/api/orders/:orderId')
    Api->>App: HandleAsync(userId, orderId)
    App->>Repo: FindByIdAsync(orderId)
    Repo-->>App: Order (PlacedAtUtc, ReceivedAtUtc)
    App->>Calc: Calculate(order.PlacedAtUtc, DateTimeOffset.UtcNow)
    Calc-->>App: status, statusHistory
    App-->>Api: OrderDetailDto { ..., status, statusHistory, receivedAtUtc }
    Api-->>UI: 200 JSON
    UI->>UI: render timeline; show "Mark as received" if Delivered and not yet received
```

```csharp
// backend/src/EPharmacy.Domain/OrderStatusCalculator.cs (shape only)
public enum OrderStatus { Placed, Processing, Shipped, Delivered }

public sealed record OrderStatusEvent(OrderStatus Status, DateTimeOffset ReachedAtUtc);

public static class OrderStatusCalculator
{
    public static OrderStatus Calculate(DateTimeOffset placedAtUtc, DateTimeOffset nowUtc);
    public static IReadOnlyList<OrderStatusEvent> History(DateTimeOffset placedAtUtc, DateTimeOffset nowUtc);
}
```

```csharp
// backend/src/EPharmacy.Domain/Order.cs (additions, shape only)
public sealed class Order
{
    // existing fields from plan 09...
    public DateTimeOffset? ReceivedAtUtc { get; }
    public void MarkReceived(DateTimeOffset nowUtc); // throws unless computed status is Delivered
}
```

```
frontend/src/
├─ pages/
│  ├─ OrdersPage.jsx                new — '/orders', RequireAuth, list view
│  └─ OrderConfirmationPage.jsx     modified — status timeline + "Mark as received"
└─ App.jsx                          modified — add '/orders' route
```

## File Changes

- [ ] `backend/src/EPharmacy.Domain/OrderStatusCalculator.cs` — new + `OrderStatus`/`OrderStatusEvent`
- [ ] `backend/tests/EPharmacy.Domain.Tests/OrderStatusCalculatorTests.cs` — new, written first (TDD)
- [ ] `backend/src/EPharmacy.Domain/Order.cs` — add `ReceivedAtUtc`, `MarkReceived`
- [ ] `backend/tests/EPharmacy.Domain.Tests/OrderTests.cs` — extend (TDD)
- [ ] `backend/src/EPharmacy.Application/GetOrderHandler.cs` — new (extracted from plan 09's inline endpoint logic) + `OrderDetailDto`
- [ ] `backend/src/EPharmacy.Application/ListOrdersHandler.cs` — new + `OrderSummaryDto`
- [ ] `backend/src/EPharmacy.Application/MarkOrderReceivedHandler.cs` — new
- [ ] `backend/tests/EPharmacy.Application.Tests/GetOrderHandlerTests.cs` — new, written first (TDD)
- [ ] `backend/tests/EPharmacy.Application.Tests/MarkOrderReceivedHandlerTests.cs` — new, written first (TDD)
- [ ] `backend/src/EPharmacy.Application/IOrderRepository.cs` — add `ListForUserAsync(userId)`
- [ ] `backend/src/EPharmacy.Infrastructure/OrderRepository.cs` — implement `ListForUserAsync`
- [ ] `backend/src/EPharmacy.Infrastructure/AppDbContext.cs` — add `ReceivedAtUtc` column to the `Order` mapping
- [ ] `backend/src/EPharmacy.Infrastructure/Migrations/*_AddOrderReceivedAt.cs` — new EF Core migration
- [ ] `backend/src/EPharmacy.Api/Endpoints/OrderEndpoints.cs` — replace inline `GET /api/orders/{orderId}` logic with `GetOrderHandler`; add `GET /api/orders`, `POST /api/orders/{orderId}/receive`
- [ ] `backend/tests/EPharmacy.Api.Tests/OrderEndpointTests.cs` — extend
- [ ] `frontend/src/pages/OrdersPage.jsx` — new + `OrdersPage.test.jsx`
- [ ] `frontend/src/pages/OrderConfirmationPage.jsx` — extend with status timeline + "Mark as received" + tests
- [ ] `frontend/src/App.jsx` — add `/orders` route (`RequireAuth`)

## Task Breakdown

1. [ ] Write `OrderStatusCalculatorTests.cs` (red): `Calculate` returns `Placed` at `t+0`, `Processing` at `t+30s`, `Shipped` at `t+90s`, `Delivered` at `t+180s`; `History` returns exactly the statuses reached so far, each with the correct `ReachedAtUtc`. Implement `OrderStatusCalculator` to go green.
2. [ ] Extend `OrderTests.cs` (red): `MarkReceived` throws when the computed status isn't `Delivered` yet (and when already received); succeeds and sets `ReceivedAtUtc` otherwise. Implement the additions to `Order.cs` to go green.
3. [ ] Write `GetOrderHandlerTests.cs` (red): returns `status`/`statusHistory` computed via `OrderStatusCalculator`, `404` (as a null/failure result) for a missing or not-owned order. Write `MarkOrderReceivedHandlerTests.cs` (red): succeeds only when `Delivered` and not yet received. Implement `GetOrderHandler`, `ListOrdersHandler`, `MarkOrderReceivedHandler` to go green.
4. [ ] Add `ListForUserAsync` to `IOrderRepository`/`OrderRepository`; add the `ReceivedAtUtc` column + migration.
5. [ ] Update `OrderEndpoints.cs`: `GET /api/orders/{orderId}` now goes through `GetOrderHandler` (still `404` for missing/not-owned); add `GET /api/orders` (list, newest-first) and `POST /api/orders/{orderId}/receive` (`409` if not yet `Delivered` or already received, `200` otherwise).
6. [ ] Extend `OrderEndpointTests.cs`: order detail includes `status`/`statusHistory`; the list endpoint returns only the caller's own orders; `receive` succeeds once `Delivered` (test by constructing an order with a `PlacedAtUtc` far enough in the past) and returns `409` before then and on a second call.
7. [ ] Build `OrdersPage.jsx` (fetches `GET /api/orders`, lists reference number/status/total/date, links to `/orders/:orderId`) + `OrdersPage.test.jsx`.
8. [ ] Extend `OrderConfirmationPage.jsx`: render the status timeline (`statusHistory`) and, when `status === 'Delivered'` and `receivedAtUtc` is null, a "Mark as received" button calling `POST /api/orders/:orderId/receive` and re-fetching; extend `OrderConfirmationPage.test.jsx` for the timeline and receive-button states.
9. [ ] Add the `/orders` route (`RequireAuth`) to `App.jsx`.
10. [ ] Full Definition of Done: `dotnet build`/`dotnet test`, `npm run build`/`lint`/`test`, manual smoke test (place an order, refresh the confirmation page a few times to watch status advance, confirm "Mark as received" appears once `Delivered` and works), `node scripts/validate-repository.mjs`.

## Commit Plan

| Tasks | Commit message | Hash |
|-------|-----------------|------|
| 1–2 | `feat(backend): add OrderStatusCalculator and MarkReceived (TDD)` | |
| 3–4 | `feat(backend): add order detail/list/receive handlers and persistence` | |
| 5–6 | `feat(backend): add GET /api/orders and POST /api/orders/{id}/receive` | |
| 7–9 | `feat(frontend): add orders list and status timeline` | |
| 10 | `docs(gen-e2): update story 10 and plan checkboxes` | |

## Acceptance Criteria Mapping

| AC | Task(s) |
|----|---------|
| Status view progresses Placed → Processing → Shipped → Delivered | 1, 5, 8 |
| Status history is visible | 1, 3, 5, 8 |
| Final delivered vs. received status is distinguished | 2, 3, 5, 6, 8 |

## Testing Strategy

| Acceptance criterion | Observable seam | Cheapest proving layer | Approach | Cadence |
|---|---|---|---|---|
| Status/history computed correctly from elapsed time | Pure function output for given timestamps | Domain unit | Test-first (TDD) | Every PR |
| `MarkReceived` invariant (only after Delivered, once) | Constructed entity / thrown exception | Domain unit | Test-first (TDD) | Every PR |
| Order detail/list/receive orchestration | Handler result under mocked ports | Application unit | Test-first (TDD) | Every PR |
| `GET /api/orders`, `GET /api/orders/{id}`, `POST .../receive` status codes | HTTP response via `WebApplicationFactory` | API functional | Test-alongside | Every PR |
| Timeline rendering, receive-button visibility/behavior | Rendered DOM under mocked `fetch` | Frontend component | Test-alongside | Every PR |
| Full browser journey watching status advance and confirming receipt | Real browser + real backend | Manual smoke | Test-after | Pre-merge manual check only |

## Dependencies

Depends on story 09 (`Order` aggregate, `GET /api/orders/{orderId}`, `OrderConfirmationPage`).

## Open Questions

- The 30s/90s/180s status-progression thresholds are an invented demo pacing — confirm they're reasonable, or adjust if a different pacing is wanted for demos/presentations.
