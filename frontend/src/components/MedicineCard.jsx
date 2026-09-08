const priceFormatter = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })

function formatPrice(priceCents) {
  return priceFormatter.format(priceCents / 100)
}

function MedicineCard({ medicine }) {
  const { name, description, priceCents, imageUrl } = medicine

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
    </article>
  )
}

export default MedicineCard
