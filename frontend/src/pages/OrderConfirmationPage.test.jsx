import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
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
  status: 'Placed',
  statusHistory: [{ status: 'Placed', reachedAtUtc: '2024-01-01T00:00:00Z' }],
  receivedAtUtc: null,
  items: [{ medicineId: 'm1', name: 'Paracetamol 500mg', unitPriceCents: 599, quantity: 2, lineTotalCents: 1198 }],
}

function mockFetch({ orderResponses }) {
  const responses = [...orderResponses]
  return vi.fn((url, options) => {
    if (url === '/api/auth/me') {
      return Promise.resolve({ ok: true, json: () => Promise.resolve(user) })
    }

    if (url === '/api/cart/summary') {
      return Promise.resolve({ ok: true, json: () => Promise.resolve({ itemCount: 0 }) })
    }

    if (url === '/api/orders/o1/receive' && options?.method === 'POST') {
      return Promise.resolve({ ok: true })
    }

    if (url === '/api/orders/o1') {
      return Promise.resolve(responses.length > 1 ? responses.shift() : responses[0])
    }

    return Promise.resolve({ ok: false, json: () => Promise.resolve(null) })
  })
}

function renderOrderConfirmationPage(orderResponse) {
  globalThis.fetch = mockFetch({ orderResponses: [orderResponse] })
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

  it('shows the status timeline', async () => {
    renderOrderConfirmationPage({ ok: true, json: () => Promise.resolve(order) })

    expect(await screen.findByTestId('order-status-timeline')).toHaveTextContent('Placed')
  })

  it('does not show the receive button before the order is delivered', async () => {
    renderOrderConfirmationPage({ ok: true, json: () => Promise.resolve(order) })

    await screen.findByText('ABC123EF')
    expect(screen.queryByRole('button', { name: /mark as received/i })).not.toBeInTheDocument()
  })

  it('shows the receive button once delivered and confirms receipt after clicking', async () => {
    const deliveredOrder = { ...order, status: 'Delivered' }
    const receivedOrder = { ...deliveredOrder, receivedAtUtc: '2024-01-01T00:05:00Z' }
    let received = false
    globalThis.fetch = vi.fn((url, options) => {
      if (url === '/api/auth/me') {
        return Promise.resolve({ ok: true, json: () => Promise.resolve(user) })
      }
      if (url === '/api/cart/summary') {
        return Promise.resolve({ ok: true, json: () => Promise.resolve({ itemCount: 0 }) })
      }
      if (url === '/api/orders/o1/receive' && options?.method === 'POST') {
        received = true
        return Promise.resolve({ ok: true })
      }
      if (url === '/api/orders/o1') {
        return Promise.resolve({ ok: true, json: () => Promise.resolve(received ? receivedOrder : deliveredOrder) })
      }
      return Promise.resolve({ ok: false, json: () => Promise.resolve(null) })
    })

    render(
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

    const button = await screen.findByRole('button', { name: /mark as received/i })
    await userEvent.click(button)

    await waitFor(() => expect(screen.getByText(/confirmed receipt/i)).toBeInTheDocument())
  })
})
