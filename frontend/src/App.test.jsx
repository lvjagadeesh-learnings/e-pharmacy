import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import App from './App'

function mockFetch({ authenticated = false } = {}) {
  return vi.fn((url) => {
    if (url === '/api/auth/me') {
      return authenticated
        ? Promise.resolve({
            ok: true,
            json: () => Promise.resolve({ id: '1', email: 'ada@example.com', displayName: 'Ada Shopper' }),
          })
        : Promise.resolve({ ok: false, json: () => Promise.resolve(null) })
    }
    if (url === '/api/auth/logout') {
      return Promise.resolve({ ok: true, json: () => Promise.resolve(null) })
    }
    if (url === '/api/medicines') {
      return Promise.resolve({ ok: true, json: () => Promise.resolve([]) })
    }
    if (url === '/api/cart/summary') {
      return Promise.resolve({ ok: true, json: () => Promise.resolve({ itemCount: 0 }) })
    }
    return Promise.resolve({
      ok: true,
      json: () => Promise.resolve({ status: 'healthy', checkedAtUtc: '2026-01-01T00:00:00Z' }),
    })
  })
}

describe('App', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('renders the e-Pharmacy shell and shows healthy once the backend responds', async () => {
    vi.stubGlobal('fetch', mockFetch())

    render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
    )

    expect(screen.getByRole('heading', { name: 'e-Pharmacy' })).toBeInTheDocument()
    expect(await screen.findByRole('status')).toHaveTextContent('healthy')
  })

  it('shows unavailable when the backend health check fails', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new Error('network error')))

    render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
    )

    expect(await screen.findByRole('status')).toHaveTextContent('unavailable')
  })

  it('shows a Log out button in the header when logged in', async () => {
    vi.stubGlobal('fetch', mockFetch({ authenticated: true }))

    render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
    )

    expect(await screen.findByRole('button', { name: 'Log out' })).toBeInTheDocument()
  })

  it('shows no Log out button in the header when logged out', async () => {
    vi.stubGlobal('fetch', mockFetch({ authenticated: false }))

    render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
    )

    await screen.findByRole('heading', { name: 'e-Pharmacy' })
    expect(screen.queryByRole('button', { name: 'Log out' })).not.toBeInTheDocument()
  })

  it('clears the session when Log out is clicked', async () => {
    const fetchMock = mockFetch({ authenticated: true })
    vi.stubGlobal('fetch', fetchMock)
    const user = userEvent.setup()

    render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
    )

    await user.click(await screen.findByRole('button', { name: 'Log out' }))

    expect(fetchMock).toHaveBeenCalledWith('/api/auth/logout', expect.objectContaining({ method: 'POST' }))
    await waitFor(() => expect(screen.queryByRole('button', { name: 'Log out' })).not.toBeInTheDocument())
  })
})

