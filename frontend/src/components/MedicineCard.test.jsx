import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import MedicineCard from './MedicineCard'

describe('MedicineCard', () => {
  it('renders the name, formatted price, and description', () => {
    render(
      <MedicineCard
        medicine={{
          id: '1',
          name: 'Paracetamol 500mg',
          description: 'Pain and fever relief tablets.',
          priceCents: 599,
          imageUrl: 'https://example.com/paracetamol.png',
        }}
      />,
    )

    expect(screen.getByRole('heading', { name: 'Paracetamol 500mg' })).toBeInTheDocument()
    expect(screen.getByText('Pain and fever relief tablets.')).toBeInTheDocument()
    expect(screen.getByText('$5.99')).toBeInTheDocument()
    expect(screen.getByRole('img', { name: 'Paracetamol 500mg' })).toHaveAttribute(
      'src',
      'https://example.com/paracetamol.png',
    )
  })

  it('renders without a broken image when imageUrl is missing', () => {
    render(
      <MedicineCard
        medicine={{
          id: '2',
          name: 'Vitamin C 1000mg',
          description: 'Immune support supplement.',
          priceCents: 899,
          imageUrl: null,
        }}
      />,
    )

    expect(screen.getByRole('img', { name: 'Vitamin C 1000mg' })).not.toHaveAttribute('src')
  })
})
