import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { useCart } from '../context/useCart'

const priceFormatter = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })

function formatPrice(priceCents) {
  return priceFormatter.format(priceCents / 100)
}

function CartPage() {
  const { refreshCount } = useCart()
  const [cart, setCart] = useState(null)
  const [loading, setLoading] = useState(true)

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

  async function handleQuantityChange(medicineId, quantity) {
    await fetch(`/api/cart/items/${medicineId}`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ quantity }),
    })
    await loadCart()
    await refreshCount()
  }

  async function handleRemove(medicineId) {
    await fetch(`/api/cart/items/${medicineId}`, { method: 'DELETE' })
    await loadCart()
    await refreshCount()
  }

  if (loading) {
    return (
      <div>
        <h2>Your cart</h2>
        <p>Loading your cart…</p>
      </div>
    )
  }

  if (!cart || cart.lines.length === 0) {
    return (
      <div>
        <h2>Your cart</h2>
        <p>Your cart is empty.</p>
        <Link to="/">Back to catalog</Link>
      </div>
    )
  }

  return (
    <div>
      <h2>Your cart</h2>
      <table>
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
              <td>{line.name}</td>
              <td>{formatPrice(line.priceCents)}</td>
              <td>
                <input
                  type="number"
                  min="0"
                  value={line.quantity}
                  aria-label={`Quantity for ${line.name}`}
                  onChange={(event) => handleQuantityChange(line.medicineId, Number(event.target.value))}
                />
              </td>
              <td>{formatPrice(line.lineTotalCents)}</td>
              <td>
                <button type="button" onClick={() => handleRemove(line.medicineId)}>
                  Remove
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      <p data-testid="cart-subtotal">Subtotal: {formatPrice(cart.subtotalCents)}</p>
      <Link to="/checkout">Proceed to checkout</Link>
    </div>
  )
}

export default CartPage
