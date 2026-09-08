import { useMedicines } from '../api/useMedicines'
import MedicineCard from '../components/MedicineCard'
import './CatalogPage.css'

function CatalogPage() {
  const { medicines, loading, error } = useMedicines()

  if (loading) {
    return (
      <div className="page-container">
        <p>Loading medicines…</p>
      </div>
    )
  }

  if (error) {
    return (
      <div className="page-container">
        <p role="alert">{error}</p>
      </div>
    )
  }

  if (medicines.length === 0) {
    return (
      <div className="page-container">
        <p>No medicines available.</p>
      </div>
    )
  }

  return (
    <div className="page-container">
      <h2>Medicine catalog</h2>
      <div className="catalog-grid">
        {medicines.map((medicine) => (
          <MedicineCard key={medicine.id} medicine={medicine} />
        ))}
      </div>
    </div>
  )
}

export default CatalogPage
