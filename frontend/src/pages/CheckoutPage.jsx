import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'

const priceFormatter = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })
function formatPrice(priceCents) {
  return priceFormatter.format(priceCents / 100)
}

function CheckoutPage() {
  const navigate = useNavigate()
  const [cart, setCart] = useState(null)
  const [loading, setLoading] = useState(true)
  const [shippingAddress, setShippingAddress] = useState('')
  const [cardNumber, setCardNumber] = useState('')
  const [expiry, setExpiry] = useState('')
  const [cvc, setCvc] = useState('')
  const [error, setError] = useState(null)
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    let cancelled = false
    fetch('/api/cart')
      .then((response) => (response.ok ? response.json() : null))
      .then((data) => {
        if (!cancelled && data) setCart(data)
        if (!cancelled) setLoading(false)
      })
      .catch(() => {
        if (!cancelled) setLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [])

  async function handleSubmit(event) {
    event.preventDefault()
    setError(null)
    setSubmitting(true)
    try {
      const response = await fetch('/api/checkout', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ shippingAddress, cardNumber, expiry, cvc }),
      })

      if (response.status === 201) {
        const body = await response.json()
        navigate(`/orders/${body.orderId}`)
        return
      }

      const body = await response.json().catch(() => null)
      setError(body?.error ?? 'Checkout failed. Please try again.')
    } catch {
      setError('Checkout failed. Please try again.')
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) {
    return (
      <div className="page-container">
        <h2>Checkout</h2>
        <p>Loading your cart…</p>
      </div>
    )
  }

  if (!cart || cart.lines.length === 0) {
    return (
      <div className="page-container">
        <h2>Checkout</h2>
        <p>Your cart is empty.</p>
      </div>
    )
  }

  return (
    <div className="page-container">
      <h2>Checkout</h2>
      <ul>
        {cart.lines.map((line) => (
          <li key={line.medicineId}>
            {line.name} x {line.quantity} — {formatPrice(line.lineTotalCents)}
          </li>
        ))}
      </ul>
      <p data-testid="checkout-subtotal">Subtotal: {formatPrice(cart.subtotalCents)}</p>
      {cart.discountCents > 0 && (
        <p data-testid="checkout-discount">Membership discount: -{formatPrice(cart.discountCents)}</p>
      )}
      <p data-testid="checkout-total">Total: {formatPrice(cart.totalCents)}</p>

      {error && (
        <p role="alert" aria-live="polite" className="form-error">
          {error}
        </p>
      )}

      <form onSubmit={handleSubmit}>
        <div className="field">
          <label className="field__label" htmlFor="shippingAddress">
            Shipping address
          </label>
          <input
            className="field__input"
            id="shippingAddress"
            value={shippingAddress}
            onChange={(event) => setShippingAddress(event.target.value)}
            required
          />
        </div>

        <div className="field">
          <label className="field__label" htmlFor="cardNumber">
            Card number
          </label>
          <input
            className="field__input"
            id="cardNumber"
            value={cardNumber}
            onChange={(event) => setCardNumber(event.target.value)}
            required
          />
        </div>

        <div className="field">
          <label className="field__label" htmlFor="expiry">
            Expiry
          </label>
          <input
            className="field__input"
            id="expiry"
            value={expiry}
            onChange={(event) => setExpiry(event.target.value)}
            required
          />
        </div>

        <div className="field">
          <label className="field__label" htmlFor="cvc">
            CVC
          </label>
          <input
            className="field__input"
            id="cvc"
            value={cvc}
            onChange={(event) => setCvc(event.target.value)}
            required
          />
        </div>

        <button type="submit" className="button button--primary" disabled={submitting}>
          Pay now
        </button>
      </form>
    </div>
  )
}

export default CheckoutPage
