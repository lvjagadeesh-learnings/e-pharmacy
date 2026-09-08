import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'

const priceFormatter = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })
function formatPrice(priceCents) {
  return priceFormatter.format(priceCents / 100)
}

function OrderConfirmationPage() {
  const { orderId } = useParams()
  const [order, setOrder] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let cancelled = false
    fetch(`/api/orders/${orderId}`)
      .then((response) => (response.ok ? response.json() : null))
      .then((data) => {
        if (!cancelled && data) setOrder(data)
        if (!cancelled) setLoading(false)
      })
      .catch(() => {
        if (!cancelled) setLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [orderId])

  if (loading) {
    return (
      <div>
        <h2>Order confirmation</h2>
        <p>Loading your order…</p>
      </div>
    )
  }

  if (!order) {
    return (
      <div>
        <h2>Order confirmation</h2>
        <p>We couldn&apos;t find that order.</p>
        <Link to="/">Back to catalog</Link>
      </div>
    )
  }

  return (
    <div>
      <h2>Thank you for your order!</h2>
      <p>
        Reference number: <strong>{order.referenceNumber}</strong>
      </p>
      <ul>
        {order.items.map((item) => (
          <li key={item.medicineId}>
            {item.name} x {item.quantity} — {formatPrice(item.lineTotalCents)}
          </li>
        ))}
      </ul>
      <p data-testid="order-total">Total: {formatPrice(order.totalCents)}</p>
      <Link to="/">Back to catalog</Link>
    </div>
  )
}

export default OrderConfirmationPage
