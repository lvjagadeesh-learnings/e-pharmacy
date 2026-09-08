import { render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import CatalogPage from './CatalogPage'

describe('CatalogPage', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('shows a loading state while the catalog is being fetched', () => {
    vi.stubGlobal('fetch', vi.fn(() => new Promise(() => {})))

    render(<CatalogPage />)

    expect(screen.getByText(/loading medicines/i)).toBeInTheDocument()
  })

  it('shows an error state when the catalog fails to load', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new Error('network error')))

    render(<CatalogPage />)

    expect(await screen.findByRole('alert')).toBeInTheDocument()
  })

  it('shows an empty state when there are no medicines', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: true, json: () => Promise.resolve([]) }))

    render(<CatalogPage />)

    expect(await screen.findByText(/no medicines available/i)).toBeInTheDocument()
  })

  it('renders a card for each medicine once loaded', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: () =>
          Promise.resolve([
            { id: '1', name: 'Paracetamol 500mg', description: 'Pain relief.', priceCents: 599, imageUrl: null },
            { id: '2', name: 'Vitamin C 1000mg', description: 'Immune support.', priceCents: 899, imageUrl: null },
          ]),
      }),
    )

    render(<CatalogPage />)

    expect(await screen.findByRole('heading', { name: 'Paracetamol 500mg' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Vitamin C 1000mg' })).toBeInTheDocument()
    expect(screen.getByText('$5.99')).toBeInTheDocument()
    expect(screen.getByText('$8.99')).toBeInTheDocument()
  })
})
