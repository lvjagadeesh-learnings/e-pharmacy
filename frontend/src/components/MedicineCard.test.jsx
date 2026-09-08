import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { AuthProvider } from '../context/AuthContext'
import { CartProvider } from '../context/CartContext'
import MedicineCard from './MedicineCard'

const medicine = {
  id: '1',
  name: 'Paracetamol 500mg',
  description: 'Pain and fever relief tablets.',
  priceCents: 599,
  imageUrl: 'https://example.com/paracetamol.png',
  averageRating: 4.5,
  reviewCount: 2,
}

function mockFetch({ authenticated }) {
  return vi.fn((url) => {
    if (url === '/api/auth/me') {
      return Promise.resolve({
        ok: authenticated,
        json: () =>
          Promise.resolve(authenticated ? { id: 'u1', email: 'ada@example.com', displayName: 'Ada' } : null),
      })
    }

    if (url === '/api/cart/summary') {
      return Promise.resolve({ ok: true, json: () => Promise.resolve({ itemCount: 0 }) })
    }

    if (url === '/api/cart/items') {
      return Promise.resolve({ ok: true, json: () => Promise.resolve({ itemCount: 1 }) })
    }

    return Promise.resolve({ ok: false, json: () => Promise.resolve(null) })
  })
}

function renderWithProviders(ui, { authenticated = false } = {}) {
  globalThis.fetch = mockFetch({ authenticated })
  return render(
    <MemoryRouter initialEntries={['/']}>
      <AuthProvider>
        <CartProvider>
          <Routes>
            <Route path="/" element={ui} />
            <Route path="/login" element={<p>Login page</p>} />
            <Route path="/medicines/:medicineId" element={<p>Medicine detail page</p>} />
          </Routes>
        </CartProvider>
      </AuthProvider>
    </MemoryRouter>,
  )
}

describe('MedicineCard', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('renders the name, formatted price, and description', () => {
    renderWithProviders(<MedicineCard medicine={medicine} />)

    expect(screen.getByRole('heading', { name: 'Paracetamol 500mg' })).toBeInTheDocument()
    expect(screen.getByText('Pain and fever relief tablets.')).toBeInTheDocument()
    expect(screen.getByText('$5.99')).toBeInTheDocument()
    expect(screen.getByRole('img', { name: 'Paracetamol 500mg' })).toHaveAttribute(
      'src',
      'https://example.com/paracetamol.png',
    )
  })

  it('renders without a broken image when imageUrl is missing', () => {
    renderWithProviders(
      <MedicineCard
        medicine={{
          id: '2',
          name: 'Vitamin C 1000mg',
          description: 'Immune support supplement.',
          priceCents: 899,
          imageUrl: null,
          averageRating: 0,
          reviewCount: 0,
        }}
      />,
    )

    expect(screen.getByRole('img', { name: 'Vitamin C 1000mg' })).not.toHaveAttribute('src')
  })

  it('shows the average rating and review count when reviews exist', () => {
    renderWithProviders(<MedicineCard medicine={medicine} />)

    expect(screen.getByText('\u2605 4.5 (2 reviews)')).toBeInTheDocument()
  })

  it('shows a no-reviews message when there are no reviews', () => {
    renderWithProviders(
      <MedicineCard medicine={{ ...medicine, averageRating: 0, reviewCount: 0 }} />,
    )

    expect(screen.getByText('No reviews yet')).toBeInTheDocument()
  })

  it('links the medicine name to the detail page', async () => {
    renderWithProviders(<MedicineCard medicine={medicine} />)
    const user = userEvent.setup()

    await user.click(screen.getByRole('link', { name: 'Paracetamol 500mg' }))

    expect(await screen.findByText('Medicine detail page')).toBeInTheDocument()
  })

  it('navigates to /login when a logged-out user clicks Add to cart', async () => {
    renderWithProviders(<MedicineCard medicine={medicine} />, { authenticated: false })
    const user = userEvent.setup()

    await user.click(await screen.findByRole('button', { name: 'Add to cart' }))

    expect(await screen.findByText('Login page')).toBeInTheDocument()
  })

  it('calls the cart API when a logged-in user clicks Add to cart', async () => {
    renderWithProviders(<MedicineCard medicine={medicine} />, { authenticated: true })
    const user = userEvent.setup()

    await user.click(await screen.findByRole('button', { name: 'Add to cart' }))

    expect(globalThis.fetch).toHaveBeenCalledWith('/api/cart/items', expect.objectContaining({ method: 'POST' }))
  })
})

