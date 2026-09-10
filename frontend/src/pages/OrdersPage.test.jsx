import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { AuthProvider } from '../context/AuthContext'
import { CartProvider } from '../context/CartContext'
import OrdersPage from './OrdersPage'

const user = { id: 'u1', email: 'ada@example.com', displayName: 'Ada' }

function mockFetch({ ordersResponse }) {
  return vi.fn((url) => {
    if (url === '/api/auth/me') {
      return Promise.resolve({ ok: true, json: () => Promise.resolve(user) })
    }

    if (url === '/api/cart/summary') {
      return Promise.resolve({ ok: true, json: () => Promise.resolve({ itemCount: 0 }) })
    }

    if (url === '/api/orders') {
      return Promise.resolve(ordersResponse)
    }

    return Promise.resolve({ ok: false, json: () => Promise.resolve(null) })
  })
}

function renderOrdersPage(ordersResponse) {
  globalThis.fetch = mockFetch({ ordersResponse })
  return render(
    <MemoryRouter initialEntries={['/orders']}>
      <AuthProvider>
        <CartProvider>
          <OrdersPage />
        </CartProvider>
      </AuthProvider>
    </MemoryRouter>,
  )
}

describe('OrdersPage', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('lists each order with reference number, status, and total', async () => {
    const orders = [
      { orderId: 'o1', referenceNumber: 'ABC123EF', placedAtUtc: '2024-01-01T00:00:00Z', totalCents: 1198, status: 'Placed' },
    ]
    renderOrdersPage({ ok: true, json: () => Promise.resolve(orders) })

    expect(await screen.findByText('ABC123EF')).toBeInTheDocument()
    expect(screen.getByText(/Placed/)).toBeInTheDocument()
    expect(screen.getByText(/\$11\.98/)).toBeInTheDocument()
  })

  it('shows an empty-state message when there are no orders', async () => {
    renderOrdersPage({ ok: true, json: () => Promise.resolve([]) })

    expect(await screen.findByText(/haven't placed any orders yet/)).toBeInTheDocument()
  })

  it('shows a distinct error message when the orders request fails', async () => {
    renderOrdersPage({ ok: false, status: 500, json: () => Promise.resolve(null) })

    expect(await screen.findByRole('alert')).toHaveTextContent(/couldn't load your orders/)
  })
})
