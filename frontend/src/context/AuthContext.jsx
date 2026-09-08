import { useState } from 'react'
import { AuthContext } from './authContextInstance'

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null)

  async function register({ email, password, displayName }) {
    const response = await fetch('/api/auth/register', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password, displayName }),
    })

    const data = await response.json().catch(() => null)

    if (!response.ok) {
      throw new Error(data?.error ?? 'Unable to create an account. Please try again.')
    }

    setUser(data)
    return data
  }

  return <AuthContext.Provider value={{ user, register }}>{children}</AuthContext.Provider>
}

