import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import HealthBanner from './HealthBanner'

describe('HealthBanner', () => {
  it('renders the given status', () => {
    render(<HealthBanner status="healthy" />)

    expect(screen.getByRole('status')).toHaveTextContent('healthy')
  })

  it('defaults to unknown when no status is provided', () => {
    render(<HealthBanner />)

    expect(screen.getByRole('status')).toHaveTextContent('unknown')
  })
})
