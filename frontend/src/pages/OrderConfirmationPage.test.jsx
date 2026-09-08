import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { AuthProvider } from '../context/AuthContext'
import { CartProvider } from '../context/CartContext'
import OrderConfirmationPage from './OrderConfirmationPage'

const user = { id: 'u1', email: 'ada@example.com', displayName: 'Ada' }

const order = {
  orderId: 'o1',
  referenceNumber: 'ABC123EF',
  shippingAddress: '1 Example St',
  totalCents: 1198,
  items: [{ medicineId: 'm1', name: 'Paracetamol 500mg', unitPriceCents: 599, quantity: 2, lineTotalCents: 1198 }],
}

function mockFetch({ orderResponse }) {
  return vi.fn((url) => {
    if (url === '/api/auth/me') {
      return Promise.resolve({ ok: true, json: () => Promise.resolve(user) })
    }

    if (url === '/api/cart/summary') {
      return Promise.resolve({ ok: true, json: () => Promise.resolve({ itemCount: 0 }) })
    }

    if (url === '/api/orders/o1') {
      return Promise.resolve(orderResponse)
    }

    return Promise.resolve({ ok: false, json: () => Promise.resolve(null) })
  })
}

function renderOrderConfirmationPage(orderResponse) {
  globalThis.fetch = mockFetch({ orderResponse })
  return render(
    <MemoryRouter initialEntries={['/orders/o1']}>
      <AuthProvider>
        <CartProvider>
          <Routes>
            <Route path="/orders/:orderId" element={<OrderConfirmationPage />} />
          </Routes>
        </CartProvider>
      </AuthProvider>
    </MemoryRouter>,
  )
}

describe('OrderConfirmationPage', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('shows the reference number, items, and total', async () => {
    renderOrderConfirmationPage({ ok: true, json: () => Promise.resolve(order) })

    expect(await screen.findByText('ABC123EF')).toBeInTheDocument()
    expect(screen.getByText(/Paracetamol 500mg/)).toBeInTheDocument()
    expect(screen.getByTestId('order-total')).toHaveTextContent('$11.98')
  })

  it('shows a not-found message when the order cannot be loaded', async () => {
    renderOrderConfirmationPage({ ok: false, json: () => Promise.resolve(null) })

    expect(await screen.findByText(/couldn't find that order/)).toBeInTheDocument()
  })
})
