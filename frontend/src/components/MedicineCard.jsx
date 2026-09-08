import { useNavigate } from 'react-router-dom'
import { useAuth } from '../context/useAuth'
import { useCart } from '../context/useCart'

const priceFormatter = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })

function formatPrice(priceCents) {
  return priceFormatter.format(priceCents / 100)
}

function MedicineCard({ medicine }) {
  const { id, name, description, priceCents, imageUrl } = medicine
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
    <article>
      <img
        src={imageUrl || undefined}
        alt={name}
        onError={(event) => {
          event.currentTarget.style.display = 'none'
        }}
      />
      <h3>{name}</h3>
      <p>{description}</p>
      <p>{formatPrice(priceCents)}</p>
      <button type="button" onClick={handleAddToCart}>
        Add to cart
      </button>
    </article>
  )
}

export default MedicineCard

