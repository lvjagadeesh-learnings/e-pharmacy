import { useEffect, useState } from 'react'

export function useHealthStatus() {
  const [status, setStatus] = useState('checking')

  useEffect(() => {
    let cancelled = false

    fetch('/health')
      .then((response) => {
        if (!response.ok) throw new Error(`Unexpected status ${response.status}`)
        return response.json()
      })
      .then((data) => {
        if (!cancelled) setStatus(data.status)
      })
      .catch(() => {
        if (!cancelled) setStatus('unavailable')
      })

    return () => {
      cancelled = true
    }
  }, [])

  return status
}
