import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../context/useAuth'

const dateFormatter = new Intl.DateTimeFormat('en-US', { dateStyle: 'long' })

function MembershipPage() {
  const { user, updateUser } = useAuth()
  const [cardNumber, setCardNumber] = useState('')
  const [expiry, setExpiry] = useState('')
  const [cvc, setCvc] = useState('')
  const [error, setError] = useState(null)
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(event) {
    event.preventDefault()
    setError(null)
    setSubmitting(true)
    try {
      const response = await fetch('/api/membership/join', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ cardNumber, expiry, cvc }),
      })

      const body = await response.json().catch(() => null)

      if (!response.ok) {
        setError(body?.error ?? 'Unable to join right now. Please try again.')
        return
      }

      updateUser({ isMember: true, membershipJoinedAtUtc: body.membershipJoinedAtUtc })
    } catch {
      setError('Unable to join right now. Please try again.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="page-container">
      <h2>e-Pharmacy Plus</h2>
      <p>
        Join <strong>e-Pharmacy Plus</strong> and get <strong>10% off every order</strong>, automatically applied at
        checkout.
      </p>

      {!user && (
        <p>
          <Link to="/login" className="button button--primary">
            Log in to join
          </Link>
        </p>
      )}

      {user && user.isMember && (
        <p data-testid="membership-status">
          You're an e-Pharmacy Plus member since {dateFormatter.format(new Date(user.membershipJoinedAtUtc))}.
        </p>
      )}

      {user && !user.isMember && (
        <>
          {error && (
            <p role="alert" aria-live="polite" className="form-error">
              {error}
            </p>
          )}

          <form onSubmit={handleSubmit}>
            <div className="field">
              <label className="field__label" htmlFor="membershipCardNumber">
                Card number
              </label>
              <input
                className="field__input"
                id="membershipCardNumber"
                value={cardNumber}
                onChange={(event) => setCardNumber(event.target.value)}
                required
              />
            </div>

            <div className="field">
              <label className="field__label" htmlFor="membershipExpiry">
                Expiry
              </label>
              <input
                className="field__input"
                id="membershipExpiry"
                value={expiry}
                onChange={(event) => setExpiry(event.target.value)}
                required
              />
            </div>

            <div className="field">
              <label className="field__label" htmlFor="membershipCvc">
                CVC
              </label>
              <input
                className="field__input"
                id="membershipCvc"
                value={cvc}
                onChange={(event) => setCvc(event.target.value)}
                required
              />
            </div>

            <button type="submit" className="button button--primary" disabled={submitting}>
              Join e-Pharmacy Plus
            </button>
          </form>
        </>
      )}
    </div>
  )
}

export default MembershipPage
