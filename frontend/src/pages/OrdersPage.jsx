import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'

const priceFormatter = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })
function formatPrice(priceCents) {
  return priceFormatter.format(priceCents / 100)
}

function OrdersPage() {
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let cancelled = false
    fetch('/api/orders')
      .then((response) => (response.ok ? response.json() : []))
      .then((data) => {
        if (!cancelled) setOrders(data ?? [])
        if (!cancelled) setLoading(false)
      })
      .catch(() => {
        if (!cancelled) setLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [])

  if (loading) {
    return (
      <div>
        <h2>Your orders</h2>
        <p>Loading your orders…</p>
      </div>
    )
  }

  if (orders.length === 0) {
    return (
      <div>
        <h2>Your orders</h2>
        <p>You haven&apos;t placed any orders yet.</p>
        <Link to="/">Back to catalog</Link>
      </div>
    )
  }

  return (
    <div>
      <h2>Your orders</h2>
      <ul>
        {orders.map((order) => (
          <li key={order.orderId}>
            <Link to={`/orders/${order.orderId}`}>{order.referenceNumber}</Link> — {order.status} —{' '}
            {formatPrice(order.totalCents)}
          </li>
        ))}
      </ul>
    </div>
  )
}

export default OrdersPage
