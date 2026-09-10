import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { AuthProvider } from '../context/AuthContext'
import { CartProvider } from '../context/CartContext'
import CatalogPage from './CatalogPage'

function renderCatalogPage() {
  return render(
    <MemoryRouter>
      <AuthProvider>
        <CartProvider>
          <CatalogPage />
        </CartProvider>
      </AuthProvider>
    </MemoryRouter>,
  )
}

describe('CatalogPage', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('shows a loading state while the catalog is being fetched', () => {
    vi.stubGlobal('fetch', vi.fn(() => new Promise(() => {})))

    renderCatalogPage()

    expect(screen.getByText(/loading medicines/i)).toBeInTheDocument()
  })

  it('shows an error state when the catalog fails to load', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new Error('network error')))

    renderCatalogPage()

    expect(await screen.findByRole('alert')).toBeInTheDocument()
  })

  it('shows an empty state when there are no medicines', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: true, json: () => Promise.resolve([]) }))

    renderCatalogPage()

    expect(await screen.findByText(/no medicines available/i)).toBeInTheDocument()
  })

  it('renders a card for each medicine once loaded', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn((url) => {
        if (url === '/api/medicines') {
          return Promise.resolve({
            ok: true,
            json: () =>
              Promise.resolve([
                { id: '1', name: 'Paracetamol 500mg', description: 'Pain relief.', priceCents: 599, imageUrl: null },
                { id: '2', name: 'Vitamin C 1000mg', description: 'Immune support.', priceCents: 899, imageUrl: null },
              ]),
          })
        }

        return Promise.resolve({ ok: false, json: () => Promise.resolve(null) })
      }),
    )

    renderCatalogPage()

    expect(await screen.findByRole('heading', { name: 'Paracetamol 500mg' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Vitamin C 1000mg' })).toBeInTheDocument()
    expect(screen.getByText('$5.99')).toBeInTheDocument()
    expect(screen.getByText('$8.99')).toBeInTheDocument()
  })
})
