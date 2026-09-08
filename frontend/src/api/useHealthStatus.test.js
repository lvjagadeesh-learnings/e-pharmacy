import { renderHook, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { useHealthStatus } from './useHealthStatus'

describe('useHealthStatus', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('starts checking, then resolves to the backend status on success', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: () => Promise.resolve({ status: 'healthy', checkedAtUtc: '2026-01-01T00:00:00Z' }),
      }),
    )

    const { result } = renderHook(() => useHealthStatus())

    expect(result.current).toBe('checking')
    await waitFor(() => expect(result.current).toBe('healthy'))
  })

  it('resolves to unavailable when the response is not ok', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }))

    const { result } = renderHook(() => useHealthStatus())

    await waitFor(() => expect(result.current).toBe('unavailable'))
  })

  it('resolves to unavailable when the fetch rejects', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new Error('network error')))

    const { result } = renderHook(() => useHealthStatus())

    await waitFor(() => expect(result.current).toBe('unavailable'))
  })
})
