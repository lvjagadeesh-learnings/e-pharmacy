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

  it('renders the e-Pharmacy shell', async () => {
    vi.stubGlobal('fetch', mockFetch())

    render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
    )

    expect(screen.getByRole('heading', { name: 'e-Pharmacy' })).toBeInTheDocument()
  })

  it('links the e-Pharmacy brand heading to the homepage', async () => {
    vi.stubGlobal('fetch', mockFetch())

    render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
    )

    expect(screen.getByRole('link', { name: 'e-Pharmacy' })).toHaveAttribute('href', '/')
  })

  it('renders a skip-to-main-content link pointing at the main landmark', async () => {
    vi.stubGlobal('fetch', mockFetch())

    render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
    )

    const skipLink = screen.getByRole('link', { name: /skip to main content/i })
    expect(skipLink).toHaveAttribute('href', '#app-main')
    expect(document.getElementById('app-main')).toBeInTheDocument()
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

  it('shows Log in and Sign up links in the header when logged out', async () => {
    vi.stubGlobal('fetch', mockFetch({ authenticated: false }))

    render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
    )

    expect(await screen.findByRole('link', { name: 'Log in' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Sign up' })).toBeInTheDocument()
    expect(screen.queryByRole('link', { name: 'Orders' })).not.toBeInTheDocument()
    expect(screen.queryByRole('link', { name: /Cart/ })).not.toBeInTheDocument()
  })

  it('hides Log in and Sign up links in the header when logged in', async () => {
    vi.stubGlobal('fetch', mockFetch({ authenticated: true }))

    render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
    )

    await screen.findByRole('button', { name: 'Log out' })
    expect(screen.queryByRole('link', { name: 'Log in' })).not.toBeInTheDocument()
    expect(screen.queryByRole('link', { name: 'Sign up' })).not.toBeInTheDocument()
  })

  it('shows a cart link with an accessible label instead of visible "Cart" text when logged in', async () => {
    vi.stubGlobal('fetch', mockFetch({ authenticated: true }))

    render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
    )

    const cartLink = await screen.findByRole('link', { name: /Cart, \d+ item/i })
    expect(cartLink).toHaveAttribute('href', '/cart')
    expect(cartLink).not.toHaveTextContent('Cart')
  })

  it("shows the signed-in user's display name in the header when logged in", async () => {
    vi.stubGlobal('fetch', mockFetch({ authenticated: true }))

    render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
    )

    expect(await screen.findByText('Ada Shopper')).toBeInTheDocument()
  })

  it('does not show a display name in the header when logged out', async () => {
    vi.stubGlobal('fetch', mockFetch({ authenticated: false }))

    render(
      <MemoryRouter>
        <App />
      </MemoryRouter>,
    )

    await screen.findByRole('heading', { name: 'e-Pharmacy' })
    expect(screen.queryByText('Ada Shopper')).not.toBeInTheDocument()
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

