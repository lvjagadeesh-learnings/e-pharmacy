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
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) {
    return (
      <div>
        <h2>Checkout</h2>
        <p>Loading your cart…</p>
      </div>
    )
  }

  if (!cart || cart.lines.length === 0) {
    return (
      <div>
        <h2>Checkout</h2>
        <p>Your cart is empty.</p>
      </div>
    )
  }

  return (
    <div>
      <h2>Checkout</h2>
      <ul>
        {cart.lines.map((line) => (
          <li key={line.medicineId}>
            {line.name} x {line.quantity} — {formatPrice(line.lineTotalCents)}
          </li>
        ))}
      </ul>
      <p data-testid="checkout-subtotal">Total: {formatPrice(cart.subtotalCents)}</p>

      {error && <p role="alert">{error}</p>}

      <form onSubmit={handleSubmit}>
        <label htmlFor="shippingAddress">Shipping address</label>
        <input
          id="shippingAddress"
          value={shippingAddress}
          onChange={(event) => setShippingAddress(event.target.value)}
          required
        />

        <label htmlFor="cardNumber">Card number</label>
        <input
          id="cardNumber"
          value={cardNumber}
          onChange={(event) => setCardNumber(event.target.value)}
          required
        />

        <label htmlFor="expiry">Expiry</label>
        <input id="expiry" value={expiry} onChange={(event) => setExpiry(event.target.value)} required />

        <label htmlFor="cvc">CVC</label>
        <input id="cvc" value={cvc} onChange={(event) => setCvc(event.target.value)} required />

        <button type="submit" disabled={submitting}>
          Pay now
        </button>
      </form>
    </div>
  )
}

export default CheckoutPage
