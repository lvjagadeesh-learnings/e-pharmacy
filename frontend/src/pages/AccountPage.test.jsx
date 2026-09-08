import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { AuthContext } from '../context/authContextInstance'
import AccountPage from './AccountPage'

describe('AccountPage', () => {
  it('shows a welcome message for the logged-in user', () => {
    render(
      <AuthContext.Provider value={{ user: { email: 'ada@example.com', displayName: 'Ada Shopper' } }}>
        <AccountPage />
      </AuthContext.Provider>,
    )

    expect(screen.getByText('Welcome, Ada Shopper.')).toBeInTheDocument()
  })
})
