import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { AuthProvider } from '../context/AuthContext'
import LoginPage from './LoginPage'

const navigateMock = vi.fn()

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom')
  return {
    ...actual,
    useNavigate: () => navigateMock,
  }
})

function renderLoginPage() {
  render(
    <MemoryRouter>
      <AuthProvider>
        <LoginPage />
      </AuthProvider>
    </MemoryRouter>,
  )
}

async function fillForm(user, { email = 'ada@example.com', password = 's3cret-password!' } = {}) {
  if (email) await user.type(screen.getByLabelText('Email'), email)
  if (password) await user.type(screen.getByLabelText('Password'), password)
}

describe('LoginPage', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
    navigateMock.mockClear()
  })

  it('shows a validation error when required fields are missing', async () => {
    const user = userEvent.setup()
    renderLoginPage()

    await user.click(screen.getByRole('button', { name: 'Log in' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Email and password are required.')
  })

  it('shows a generic error for invalid credentials', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: false,
        json: () => Promise.resolve({ message: 'Invalid email or password.' }),
      }),
    )

    const user = userEvent.setup()
    renderLoginPage()
    await fillForm(user)

    await user.click(screen.getByRole('button', { name: 'Log in' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Invalid email or password.')
  })

  it('redirects to / after a successful login', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: () => Promise.resolve({ id: '1', email: 'ada@example.com', displayName: 'Ada Shopper' }),
      }),
    )

    const user = userEvent.setup()
    renderLoginPage()
    await fillForm(user)

    await user.click(screen.getByRole('button', { name: 'Log in' }))

    await waitFor(() => expect(navigateMock).toHaveBeenCalledWith('/'))
  })
})
