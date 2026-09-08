import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { AuthProvider } from '../context/AuthContext'
import { CartProvider } from '../context/CartContext'
import CartPage from './CartPage'

const user = { id: 'u1', email: 'ada@example.com', displayName: 'Ada' }

const singleLineCart = {
  lines: [
    { medicineId: 'm1', name: 'Paracetamol 500mg', priceCents: 599, quantity: 2, lineTotalCents: 1198 },
  ],
  subtotalCents: 1198,
}

const emptyCart = { lines: [], subtotalCents: 0 }

function mockFetch({ cart }) {
  return vi.fn((url) => {
    if (url === '/api/auth/me') {
      return Promise.resolve({ ok: true, json: () => Promise.resolve(user) })
    }

    if (url === '/api/cart/summary') {
      return Promise.resolve({ ok: true, json: () => Promise.resolve({ itemCount: cart.lines.length }) })
    }

    if (url === '/api/cart') {
      return Promise.resolve({ ok: true, json: () => Promise.resolve(cart) })
    }

    if (typeof url === 'string' && url.startsWith('/api/cart/items/')) {
      return Promise.resolve({ ok: true, status: 204, json: () => Promise.resolve(null) })
    }

    return Promise.resolve({ ok: false, json: () => Promise.resolve(null) })
  })
}

function renderCartPage(cart) {
  globalThis.fetch = mockFetch({ cart })
  return render(
    <MemoryRouter initialEntries={['/cart']}>
      <AuthProvider>
        <CartProvider>
          <CartPage />
        </CartProvider>
      </AuthProvider>
    </MemoryRouter>,
  )
}

describe('CartPage', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('renders each cart line with name, unit price, quantity, and line total', async () => {
    renderCartPage(singleLineCart)

    expect(await screen.findByText('Paracetamol 500mg')).toBeInTheDocument()
    expect(screen.getByText('$5.99')).toBeInTheDocument()
    expect(screen.getByLabelText('Quantity for Paracetamol 500mg')).toHaveValue(2)
    expect(screen.getByText('$11.98')).toBeInTheDocument()
    expect(screen.getByTestId('cart-subtotal')).toHaveTextContent('$11.98')
  })

  it('shows an empty-cart message with a link back to the catalog', async () => {
    renderCartPage(emptyCart)

    expect(await screen.findByText('Your cart is empty.')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Back to catalog' })).toHaveAttribute('href', '/')
  })

  it('calls the PATCH endpoint when the quantity is changed', async () => {
    renderCartPage(singleLineCart)
    await screen.findByText('Paracetamol 500mg')

    const quantityInput = screen.getByLabelText('Quantity for Paracetamol 500mg')
    await userEvent.clear(quantityInput)
    await userEvent.type(quantityInput, '5')

    await waitFor(() => {
      expect(globalThis.fetch).toHaveBeenCalledWith(
        '/api/cart/items/m1',
        expect.objectContaining({ method: 'PATCH' }),
      )
    })
  })

  it('calls the DELETE endpoint when the remove button is clicked', async () => {
    renderCartPage(singleLineCart)
    await screen.findByText('Paracetamol 500mg')

    await userEvent.click(screen.getByRole('button', { name: 'Remove' }))

    await waitFor(() => {
      expect(globalThis.fetch).toHaveBeenCalledWith(
        '/api/cart/items/m1',
        expect.objectContaining({ method: 'DELETE' }),
      )
    })
  })
})
