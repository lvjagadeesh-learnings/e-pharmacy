import { useMedicines } from '../api/useMedicines'
import MedicineCard from '../components/MedicineCard'

function CatalogPage() {
  const { medicines, loading, error } = useMedicines()

  if (loading) {
    return <p>Loading medicines…</p>
  }

  if (error) {
    return <p role="alert">{error}</p>
  }

  if (medicines.length === 0) {
    return <p>No medicines available.</p>
  }

  return (
    <div>
      <h2>Medicine catalog</h2>
      <div>
        {medicines.map((medicine) => (
          <MedicineCard key={medicine.id} medicine={medicine} />
        ))}
      </div>
    </div>
  )
}

export default CatalogPage
