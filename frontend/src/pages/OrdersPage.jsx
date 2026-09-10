import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'

const priceFormatter = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })
function formatPrice(priceCents) {
  return priceFormatter.format(priceCents / 100)
}

function OrdersPage() {
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState(false)

  useEffect(() => {
    let cancelled = false
    fetch('/api/orders')
      .then((response) => {
        if (!response.ok) throw new Error('Failed to load orders')
        return response.json()
      })
      .then((data) => {
        if (cancelled) return
        setOrders(data ?? [])
        setLoading(false)
      })
      .catch(() => {
        if (cancelled) return
        setLoadError(true)
        setLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [])

  if (loading) {
    return (
      <div className="page-container">
        <h2>Your orders</h2>
        <p>Loading your orders…</p>
      </div>
    )
  }

  if (loadError) {
    return (
      <div className="page-container">
        <h2>Your orders</h2>
        <p role="alert">We couldn&apos;t load your orders right now. Please try again.</p>
      </div>
    )
  }

  if (orders.length === 0) {
    return (
      <div className="page-container">
        <h2>Your orders</h2>
        <p>You haven&apos;t placed any orders yet.</p>
        <Link to="/">Back to catalog</Link>
      </div>
    )
  }

  return (
    <div className="page-container">
      <h2>Your orders</h2>
      <ul className="orders-list">
        {orders.map((order) => (
          <li key={order.orderId} className="card">
            <Link to={`/orders/${order.orderId}`}>{order.referenceNumber}</Link> — {order.status} —{' '}
            {formatPrice(order.totalCents)}
          </li>
        ))}
      </ul>
    </div>
  )
}

export default OrdersPage
