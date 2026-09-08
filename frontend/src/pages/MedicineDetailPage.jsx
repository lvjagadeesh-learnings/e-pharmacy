import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { useAuth } from '../context/useAuth'

const priceFormatter = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })
function formatPrice(priceCents) {
  return priceFormatter.format(priceCents / 100)
}

function MedicineDetailPage() {
  const { medicineId } = useParams()
  const { user } = useAuth()
  const [medicine, setMedicine] = useState(null)
  const [reviews, setReviews] = useState([])
  const [loading, setLoading] = useState(true)
  const [rating, setRating] = useState('5')
  const [comment, setComment] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [submitError, setSubmitError] = useState(null)

  useEffect(() => {
    let cancelled = false

    Promise.all([
      fetch('/api/medicines').then((response) => (response.ok ? response.json() : [])),
      fetch(`/api/medicines/${medicineId}/reviews`).then((response) => (response.ok ? response.json() : [])),
    ])
      .then(([medicines, medicineReviews]) => {
        if (cancelled) return
        setMedicine(medicines.find((m) => m.id === medicineId) ?? null)
        setReviews(medicineReviews)
      })
      .catch(() => {
        if (!cancelled) setMedicine(null)
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [medicineId])

  function handleSubmit(event) {
    event.preventDefault()
    setSubmitting(true)
    setSubmitError(null)

    fetch(`/api/medicines/${medicineId}/reviews`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ rating: Number(rating), comment }),
    })
      .then((response) => {
        if (!response.ok) {
          return response.json().then((body) => {
            throw new Error(body?.error ?? 'Unable to submit your review.')
          })
        }
        return response.json()
      })
      .then((newReview) => {
        setReviews((current) => [newReview, ...current])
        setComment('')
      })
      .catch((error) => {
        setSubmitError(error.message)
      })
      .finally(() => {
        setSubmitting(false)
      })
  }

  if (loading) {
    return <p>Loading medicine…</p>
  }

  if (!medicine) {
    return (
      <p role="alert">
        Medicine not found. <Link to="/">Back to catalog</Link>
      </p>
    )
  }

  return (
    <div className="page-container">
      <h2>{medicine.name}</h2>
      <p>{medicine.description}</p>
      <p>{formatPrice(medicine.priceCents)}</p>

      <h3>Reviews</h3>
      {reviews.length === 0 ? (
        <p>No reviews yet.</p>
      ) : (
        <ul>
          {reviews.map((review) => (
            <li key={review.id} className="card">
              <strong>{review.reviewerDisplayName}</strong> — {review.rating}/5
              <p>{review.comment}</p>
            </li>
          ))}
        </ul>
      )}

      {user && (
        <form onSubmit={handleSubmit}>
          <h3>Leave a review</h3>
          <div className="field">
            <label className="field__label" htmlFor="rating">
              Rating
            </label>
            <select
              className="field__input"
              id="rating"
              value={rating}
              onChange={(event) => setRating(event.target.value)}
            >
              <option value="5">5</option>
              <option value="4">4</option>
              <option value="3">3</option>
              <option value="2">2</option>
              <option value="1">1</option>
            </select>
          </div>
          <div className="field">
            <label className="field__label" htmlFor="comment">
              Comment
            </label>
            <textarea
              className="field__input"
              id="comment"
              value={comment}
              onChange={(event) => setComment(event.target.value)}
              required
            />
          </div>
          {submitError && (
            <p role="alert" aria-live="polite" className="form-error">
              {submitError}
            </p>
          )}
          <button type="submit" className="button button--primary" disabled={submitting}>
            Submit review
          </button>
        </form>
      )}
    </div>
  )
}

export default MedicineDetailPage
