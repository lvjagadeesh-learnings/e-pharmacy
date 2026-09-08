import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { AuthProvider } from '../context/AuthContext'
import { CartProvider } from '../context/CartContext'
import MedicineDetailPage from './MedicineDetailPage'

const user = { id: 'u1', email: 'ada@example.com', displayName: 'Ada' }

const medicines = [
  { id: 'm1', name: 'Paracetamol 500mg', description: 'Pain and fever relief tablets.', priceCents: 599, imageUrl: null, averageRating: 4, reviewCount: 1 },
]

const existingReviews = [
  { id: 'r1', reviewerDisplayName: 'Bob Shopper', rating: 4, comment: 'Worked well.', createdAtUtc: '2024-01-01T00:00:00Z' },
]

function renderPage({ authenticated, reviews = existingReviews, submitResponse } = {}) {
  globalThis.fetch = vi.fn((url, options) => {
    if (url === '/api/auth/me') {
      return Promise.resolve({ ok: authenticated, json: () => Promise.resolve(authenticated ? user : null) })
    }

    if (url === '/api/cart/summary') {
      return Promise.resolve({ ok: true, json: () => Promise.resolve({ itemCount: 0 }) })
    }

    if (url === '/api/medicines') {
      return Promise.resolve({ ok: true, json: () => Promise.resolve(medicines) })
    }

    if (url === '/api/medicines/m1/reviews' && options?.method === 'POST') {
      return Promise.resolve(
        submitResponse ?? {
          ok: true,
          json: () =>
            Promise.resolve({ id: 'r2', reviewerDisplayName: 'Ada', rating: 5, comment: 'Great!', createdAtUtc: '2024-01-02T00:00:00Z' }),
        },
      )
    }

    if (url === '/api/medicines/m1/reviews') {
      return Promise.resolve({ ok: true, json: () => Promise.resolve(reviews) })
    }

    return Promise.resolve({ ok: false, json: () => Promise.resolve(null) })
  })

  return render(
    <MemoryRouter initialEntries={['/medicines/m1']}>
      <AuthProvider>
        <CartProvider>
          <Routes>
            <Route path="/medicines/:medicineId" element={<MedicineDetailPage />} />
          </Routes>
        </CartProvider>
      </AuthProvider>
    </MemoryRouter>,
  )
}

describe('MedicineDetailPage', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('shows the medicine details and existing reviews', async () => {
    renderPage({ authenticated: false })

    expect(await screen.findByRole('heading', { name: 'Paracetamol 500mg' })).toBeInTheDocument()
    expect(screen.getByText('Pain and fever relief tablets.')).toBeInTheDocument()
    expect(screen.getByText(/Bob Shopper/)).toBeInTheDocument()
    expect(screen.getByText('Worked well.')).toBeInTheDocument()
  })

  it('does not show the review form when logged out', async () => {
    renderPage({ authenticated: false })

    await screen.findByRole('heading', { name: 'Paracetamol 500mg' })
    expect(screen.queryByRole('button', { name: /submit review/i })).not.toBeInTheDocument()
  })

  it('shows the review form when logged in and submits successfully', async () => {
    renderPage({ authenticated: true })
    const userEventInstance = userEvent.setup()

    await screen.findByRole('heading', { name: 'Paracetamol 500mg' })
    await userEventInstance.type(screen.getByLabelText('Comment'), 'Great product!')
    await userEventInstance.click(screen.getByRole('button', { name: /submit review/i }))

    await waitFor(() => expect(screen.getByText('Great!')).toBeInTheDocument())
  })

  it('shows an inline error when submission fails', async () => {
    renderPage({
      authenticated: true,
      submitResponse: { ok: false, json: () => Promise.resolve({ error: "You've already reviewed this item." }) },
    })
    const userEventInstance = userEvent.setup()

    await screen.findByRole('heading', { name: 'Paracetamol 500mg' })
    await userEventInstance.type(screen.getByLabelText('Comment'), 'Great product!')
    await userEventInstance.click(screen.getByRole('button', { name: /submit review/i }))

    expect(await screen.findByRole('alert')).toHaveTextContent("You've already reviewed this item.")
  })
})
