import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/useAuth'
import { useCart } from '../context/useCart'

const priceFormatter = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })

function formatPrice(priceCents) {
  return priceFormatter.format(priceCents / 100)
}

function MedicineCard({ medicine }) {
  const { id, name, description, priceCents, imageUrl, averageRating, reviewCount } = medicine
  const { user } = useAuth()
  const { addItem } = useCart()
  const navigate = useNavigate()

  function handleAddToCart() {
    if (!user) {
      navigate('/login')
      return
    }

    addItem(id, 1)
  }

  return (
    <article className="card">
      <img
        src={imageUrl || undefined}
        alt={name}
        onError={(event) => {
          event.currentTarget.style.display = 'none'
        }}
      />
      <h3>
        <Link to={`/medicines/${id}`}>{name}</Link>
      </h3>
      <p>{description}</p>
      <p>{formatPrice(priceCents)}</p>
      <p>
        {reviewCount > 0
          ? `\u2605 ${averageRating.toFixed(1)} (${reviewCount} review${reviewCount === 1 ? '' : 's'})`
          : 'No reviews yet'}
      </p>
      <button type="button" className="button button--primary" onClick={handleAddToCart}>
        Add to cart
      </button>
    </article>
  )
}

export default MedicineCard

