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
  const [loadError, setLoadError] = useState(false)
  const [receiving, setReceiving] = useState(false)
  const [receiveError, setReceiveError] = useState(null)

  useEffect(() => {
    let cancelled = false
    fetch(`/api/orders/${orderId}`)
      .then((response) => {
        if (response.ok) return response.json()
        if (response.status === 404) return null
        throw new Error('Failed to load order')
      })
      .then((data) => {
        if (cancelled) return
        setOrder(data)
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
  }, [orderId])

  function handleMarkReceived() {
    setReceiving(true)
    setReceiveError(null)
    fetch(`/api/orders/${orderId}/receive`, { method: 'POST' })
      .then((response) => {
        if (!response.ok) {
          setReceiving(false)
          setReceiveError('Unable to mark this order as received.')
          return null
        }
        return fetch(`/api/orders/${orderId}`).then((r) => (r.ok ? r.json() : null))
      })
      .then((data) => {
        if (data) setOrder(data)
        setReceiving(false)
      })
      .catch(() => {
        setReceiving(false)
        setReceiveError('Unable to mark this order as received.')
      })
  }

  if (loading) {
    return (
      <div className="page-container">
        <h2>Order confirmation</h2>
        <p>Loading your order…</p>
      </div>
    )
  }

  if (loadError) {
    return (
      <div className="page-container">
        <h2>Order confirmation</h2>
        <p role="alert">We couldn&apos;t load this order right now. Please try again.</p>
        <Link to="/">Back to catalog</Link>
      </div>
    )
  }

  if (!order) {
    return (
      <div className="page-container">
        <h2>Order confirmation</h2>
        <p>We couldn&apos;t find that order.</p>
        <Link to="/">Back to catalog</Link>
      </div>
    )
  }

  return (
    <div className="page-container">
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

      {order.statusHistory && (
        <div>
          <h3>Order status</h3>
          <ul className="status-timeline" data-testid="order-status-timeline">
            {order.statusHistory.map((event) => (
              <li key={event.status}>{event.status}</li>
            ))}
          </ul>
        </div>
      )}

      {order.status === 'Delivered' && !order.receivedAtUtc && (
        <button type="button" className="button button--primary" onClick={handleMarkReceived} disabled={receiving}>
          Mark as received
        </button>
      )}
      {order.receivedAtUtc && <p>You confirmed receipt of this order.</p>}
      {receiveError && (
        <p role="alert" aria-live="polite" className="form-error">
          {receiveError}
        </p>
      )}

      <Link to="/">Back to catalog</Link>
    </div>
  )
}

export default OrderConfirmationPage
