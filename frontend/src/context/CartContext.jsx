import { useEffect, useState } from 'react'
import { CartContext } from './cartContextInstance'
import { useAuth } from './useAuth'

export function CartProvider({ children }) {
  const { user, loading: authLoading } = useAuth()
  const [itemCount, setItemCount] = useState(0)

  useEffect(() => {
    if (authLoading || !user) {
      return undefined
    }

    let cancelled = false

    fetch('/api/cart/summary')
      .then((response) => (response.ok ? response.json() : null))
      .then((data) => {
        if (!cancelled && data) setItemCount(data.itemCount)
      })
      .catch(() => {
        if (!cancelled) setItemCount(0)
      })

    return () => {
      cancelled = true
    }
  }, [authLoading, user])

  async function refreshCount() {
    const response = await fetch('/api/cart/summary')
    if (!response.ok) {
      return
    }

    const data = await response.json().catch(() => null)
    if (data) {
      setItemCount(data.itemCount)
    }
  }

  async function addItem(medicineId, quantity = 1) {
    const response = await fetch('/api/cart/items', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ medicineId, quantity }),
    })

    const data = await response.json().catch(() => null)

    if (!response.ok) {
      throw new Error(data?.error ?? 'Unable to add this medicine to your cart.')
    }

    setItemCount(data.itemCount)
    return data
  }

  return <CartContext.Provider value={{ itemCount, addItem, refreshCount }}>{children}</CartContext.Provider>
}
