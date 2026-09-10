import { useCallback, useEffect, useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import { useCart } from '../context/useCart'

const priceFormatter = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })
const QUANTITY_COMMIT_DELAY_MS = 400

function formatPrice(priceCents) {
  return priceFormatter.format(priceCents / 100)
}

function CartPage() {
  const { refreshCount } = useCart()
  const [cart, setCart] = useState(null)
  const [loading, setLoading] = useState(true)
  const [quantityDrafts, setQuantityDrafts] = useState({})
  const commitTimers = useRef({})

  const loadCart = useCallback(() => {
    setLoading(true)
    return fetch('/api/cart')
      .then((response) => (response.ok ? response.json() : null))
      .then((data) => {
        if (data) setCart(data)
        setLoading(false)
      })
      .catch(() => {
        setLoading(false)
      })
  }, [])

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

  useEffect(() => {
    const timers = commitTimers.current
    return () => {
      Object.values(timers).forEach(clearTimeout)
    }
  }, [])

  async function commitQuantityChange(medicineId, quantity) {
    await fetch(`/api/cart/items/${medicineId}`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ quantity }),
    })
    await loadCart()
    await refreshCount()
    setQuantityDrafts((prev) => {
      const rest = { ...prev }
      delete rest[medicineId]
      return rest
    })
  }

  function handleQuantityChange(medicineId, quantity) {
    setQuantityDrafts((prev) => ({ ...prev, [medicineId]: quantity }))

    if (commitTimers.current[medicineId]) {
      clearTimeout(commitTimers.current[medicineId])
    }

    commitTimers.current[medicineId] = setTimeout(() => {
      delete commitTimers.current[medicineId]
      commitQuantityChange(medicineId, quantity)
    }, QUANTITY_COMMIT_DELAY_MS)
  }

  async function handleRemove(medicineId) {
    await fetch(`/api/cart/items/${medicineId}`, { method: 'DELETE' })
    await loadCart()
    await refreshCount()
  }

  if (loading) {
    return (
      <div className="page-container">
        <h2>Your cart</h2>
        <p>Loading your cart…</p>
      </div>
    )
  }

  if (!cart || cart.lines.length === 0) {
    return (
      <div className="page-container">
        <h2>Your cart</h2>
        <p>Your cart is empty.</p>
        <Link to="/">Back to catalog</Link>
      </div>
    )
  }

  return (
    <div className="page-container">
      <h2>Your cart</h2>
      <table className="responsive-table">
        <thead>
          <tr>
            <th>Medicine</th>
            <th>Unit price</th>
            <th>Quantity</th>
            <th>Line total</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {cart.lines.map((line) => (
            <tr key={line.medicineId}>
              <td data-label="Medicine">{line.name}</td>
              <td data-label="Unit price">{formatPrice(line.priceCents)}</td>
              <td data-label="Quantity">
                <input
                  className="field__input"
                  type="number"
                  min="0"
                  value={quantityDrafts[line.medicineId] ?? line.quantity}
                  aria-label={`Quantity for ${line.name}`}
                  onChange={(event) => handleQuantityChange(line.medicineId, Number(event.target.value))}
                />
              </td>
              <td data-label="Line total">{formatPrice(line.lineTotalCents)}</td>
              <td data-label="">
                <button type="button" className="button button--secondary" onClick={() => handleRemove(line.medicineId)}>
                  Remove
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      <p data-testid="cart-subtotal">Subtotal: {formatPrice(cart.subtotalCents)}</p>
      <Link to="/checkout" className="button button--primary">
        Proceed to checkout
      </Link>
    </div>
  )
}

export default CartPage
