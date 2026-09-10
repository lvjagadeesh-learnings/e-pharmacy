import { useEffect, useState } from 'react'

export function useMedicines() {
  const [medicines, setMedicines] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    let cancelled = false

    fetch('/api/medicines')
      .then((response) => {
        if (!response.ok) throw new Error(`Unexpected status ${response.status}`)
        return response.json()
      })
      .then((data) => {
        if (!cancelled) {
          setMedicines(data)
          setError(null)
        }
      })
      .catch(() => {
        if (!cancelled) setError('Unable to load the medicine catalog. Please try again later.')
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [])

  return { medicines, loading, error }
}
