import { renderHook, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { useMedicines } from './useMedicines'

const sampleMedicines = [
  { id: '1', name: 'Paracetamol 500mg', description: 'Pain relief tablets.', priceCents: 599, imageUrl: null },
]

describe('useMedicines', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('starts loading, then resolves to the fetched medicines on success', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: () => Promise.resolve(sampleMedicines),
      }),
    )

    const { result } = renderHook(() => useMedicines())

    expect(result.current.loading).toBe(true)
    expect(result.current.medicines).toEqual([])
    expect(result.current.error).toBeNull()

    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(result.current.medicines).toEqual(sampleMedicines)
    expect(result.current.error).toBeNull()
  })

  it('resolves to an empty list when the backend returns none', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: true, json: () => Promise.resolve([]) }))

    const { result } = renderHook(() => useMedicines())

    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(result.current.medicines).toEqual([])
    expect(result.current.error).toBeNull()
  })

  it('sets an error when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 500 }))

    const { result } = renderHook(() => useMedicines())

    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(result.current.error).toBeTruthy()
  })

  it('sets an error when the fetch rejects', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new Error('network error')))

    const { result } = renderHook(() => useMedicines())

    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(result.current.error).toBeTruthy()
  })
})
