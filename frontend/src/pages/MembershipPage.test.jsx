import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { AuthContext } from '../context/authContextInstance'
import { AuthProvider } from '../context/AuthContext'
import MembershipPage from './MembershipPage'

function renderWithAuthValue(value) {
  return render(
    <MemoryRouter>
      <AuthContext.Provider value={value}>
        <MembershipPage />
      </AuthContext.Provider>
    </MemoryRouter>,
  )
}

describe('MembershipPage', () => {
  it('shows a login prompt and no form when logged out', () => {
    renderWithAuthValue({ user: null, updateUser: vi.fn() })

    expect(screen.getByText(/10% off every order/i)).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /log in to join/i })).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /join e-pharmacy plus/i })).not.toBeInTheDocument()
  })

  it('shows a join form for a logged-in non-member', () => {
    renderWithAuthValue({ user: { id: 'u1', isMember: false }, updateUser: vi.fn() })

    expect(screen.getByRole('button', { name: /join e-pharmacy plus/i })).toBeInTheDocument()
  })

  it('shows the membership-since status for an existing member', () => {
    renderWithAuthValue({ user: { id: 'u1', isMember: true, membershipJoinedAtUtc: '2024-01-15T00:00:00Z' }, updateUser: vi.fn() })

    expect(screen.getByTestId('membership-status')).toHaveTextContent('member since January 15, 2024')
    expect(screen.queryByRole('button', { name: /join e-pharmacy plus/i })).not.toBeInTheDocument()
  })

  describe('joining', () => {
    beforeEach(() => {
      vi.stubGlobal('fetch', vi.fn())
    })

    afterEach(() => {
      vi.unstubAllGlobals()
    })

    it('marks the user a member on a successful join', async () => {
      const user = { id: 'u1', email: 'ada@example.com', displayName: 'Ada', isMember: false }
      globalThis.fetch = vi.fn((url, options) => {
        if (url === '/api/auth/me') {
          return Promise.resolve({ ok: true, json: () => Promise.resolve(user) })
        }
        if (url === '/api/membership/join' && options?.method === 'POST') {
          return Promise.resolve({
            ok: true,
            json: () => Promise.resolve({ isMember: true, membershipJoinedAtUtc: '2024-03-01T00:00:00Z' }),
          })
        }
        return Promise.resolve({ ok: false, json: () => Promise.resolve(null) })
      })

      render(
        <MemoryRouter>
          <AuthProvider>
            <MembershipPage />
          </AuthProvider>
        </MemoryRouter>,
      )

      await waitFor(() => expect(screen.getByRole('button', { name: /join e-pharmacy plus/i })).toBeInTheDocument())

      const events = userEvent.setup()
      await events.type(screen.getByLabelText(/card number/i), '4111111111111111')
      await events.type(screen.getByLabelText(/expiry/i), '12/30')
      await events.type(screen.getByLabelText(/cvc/i), '123')
      await events.click(screen.getByRole('button', { name: /join e-pharmacy plus/i }))

      await waitFor(() => expect(screen.getByTestId('membership-status')).toBeInTheDocument())
    })
  })
})
