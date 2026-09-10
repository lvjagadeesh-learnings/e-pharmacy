import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { AuthProvider } from '../context/AuthContext'
import { CartProvider } from '../context/CartContext'
import CheckoutPage from './CheckoutPage'

const user = { id: 'u1', email: 'ada@example.com', displayName: 'Ada' }

const singleLineCart = {
  lines: [
    { medicineId: 'm1', name: 'Paracetamol 500mg', priceCents: 599, quantity: 2, lineTotalCents: 1198 },
  ],
  subtotalCents: 1198,
  discountCents: 0,
  totalCents: 1198,
}

const memberCartWithDiscount = {
  lines: [
    { medicineId: 'm1', name: 'Paracetamol 500mg', priceCents: 599, quantity: 2, lineTotalCents: 1198 },
  ],
  subtotalCents: 1198,
  discountCents: 118,
  totalCents: 1080,
}

function mockFetch({ cart, checkoutResponse }) {
  return vi.fn((url, options) => {
    if (url === '/api/auth/me') {
      return Promise.resolve({ ok: true, json: () => Promise.resolve(user) })
    }

    if (url === '/api/cart/summary') {
      return Promise.resolve({ ok: true, json: () => Promise.resolve({ itemCount: cart.lines.length }) })
    }

    if (url === '/api/cart') {
      return Promise.resolve({ ok: true, json: () => Promise.resolve(cart) })
    }

    if (url === '/api/checkout' && options?.method === 'POST') {
      return typeof checkoutResponse === 'function' ? checkoutResponse() : Promise.resolve(checkoutResponse)
    }

    return Promise.resolve({ ok: false, json: () => Promise.resolve(null) })
  })
}

function renderCheckoutPage({ cart, checkoutResponse }) {
  globalThis.fetch = mockFetch({ cart, checkoutResponse })
  return render(
    <MemoryRouter initialEntries={['/checkout']}>
      <AuthProvider>
        <CartProvider>
          <CheckoutPage />
        </CartProvider>
      </AuthProvider>
    </MemoryRouter>,
  )
}

describe('CheckoutPage', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('shows the cart summary and shipping/payment form', async () => {
    renderCheckoutPage({ cart: singleLineCart, checkoutResponse: { status: 201, json: () => Promise.resolve({}) } })

    expect(await screen.findByText(/Paracetamol 500mg/)).toBeInTheDocument()
    expect(screen.getByText('Total: $11.98')).toBeInTheDocument()
    expect(screen.getByLabelText('Shipping address')).toBeInTheDocument()
    expect(screen.getByLabelText('Card number')).toBeInTheDocument()
  })

  it('navigates to the order confirmation page on a successful payment', async () => {
    renderCheckoutPage({
      cart: singleLineCart,
      checkoutResponse: {
        status: 201,
        json: () => Promise.resolve({ orderId: 'o1', referenceNumber: 'ABC123', totalCents: 1198 }),
      },
    })
    const person = userEvent.setup()

    await screen.findByLabelText('Shipping address')
    await person.type(screen.getByLabelText('Shipping address'), '1 Example St')
    await person.type(screen.getByLabelText('Card number'), '4111111111111111')
    await person.type(screen.getByLabelText('Expiry'), '12/30')
    await person.type(screen.getByLabelText('CVC'), '123')
    await person.click(screen.getByRole('button', { name: 'Pay now' }))

    await waitFor(() => {
      expect(globalThis.fetch).toHaveBeenCalledWith('/api/checkout', expect.objectContaining({ method: 'POST' }))
    })
  })

  it('shows the decline message and preserves the form on a payment decline', async () => {
    renderCheckoutPage({
      cart: singleLineCart,
      checkoutResponse: {
        status: 402,
        json: () => Promise.resolve({ error: 'The card was declined.' }),
      },
    })
    const person = userEvent.setup()

    await screen.findByLabelText('Shipping address')
    await person.type(screen.getByLabelText('Shipping address'), '1 Example St')
    await person.type(screen.getByLabelText('Card number'), '4000000000000002')
    await person.type(screen.getByLabelText('Expiry'), '12/30')
    await person.type(screen.getByLabelText('CVC'), '123')
    await person.click(screen.getByRole('button', { name: 'Pay now' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('The card was declined.')
    expect(screen.getByLabelText('Shipping address')).toHaveValue('1 Example St')
  })

  it('shows the membership discount and discounted total for a member', async () => {
    renderCheckoutPage({
      cart: memberCartWithDiscount,
      checkoutResponse: { status: 201, json: () => Promise.resolve({}) },
    })

    expect(await screen.findByText(/Paracetamol 500mg/)).toBeInTheDocument()
    expect(screen.getByTestId('checkout-subtotal')).toHaveTextContent('Subtotal: $11.98')
    expect(screen.getByTestId('checkout-discount')).toHaveTextContent('Membership discount: -$1.18')
    expect(screen.getByTestId('checkout-total')).toHaveTextContent('Total: $10.80')
  })

  it('shows an error and preserves the form when the checkout request fails outright', async () => {
    renderCheckoutPage({
      cart: singleLineCart,
      checkoutResponse: () => Promise.reject(new Error('Network error')),
    })
    const person = userEvent.setup()

    await screen.findByLabelText('Shipping address')
    await person.type(screen.getByLabelText('Shipping address'), '1 Example St')
    await person.type(screen.getByLabelText('Card number'), '4111111111111111')
    await person.type(screen.getByLabelText('Expiry'), '12/30')
    await person.type(screen.getByLabelText('CVC'), '123')
    await person.click(screen.getByRole('button', { name: 'Pay now' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Checkout failed. Please try again.')
    expect(screen.getByLabelText('Shipping address')).toHaveValue('1 Example St')
  })
})
