# User Story: 09 — Check out and pay to place an order

**Status:** ready

**As a** shopper with items in my cart,
**I want** to check out, pay with a simulated payment method, and get an order confirmation,
**so that** I can complete my purchase and know the order was placed.

## Acceptance Criteria

- [ ] A checkout flow collects/confirms a delivery address and shows an order summary with total
- [ ] A fake/simulated payment step accepts mock payment details (no real payment gateway) and simulates success or failure
- [ ] On simulated payment success, an order is created and the cart is cleared
- [ ] The user sees an order-placed confirmation with an order reference/number
- [ ] On simulated payment failure, the user sees an error and the cart is preserved so they can retry

## Notes

- "Fake payment transaction" is explicitly a simulated/mock integration — no real payment provider is in scope.
- Depends on story 08 (cart).
