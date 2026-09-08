import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { AuthProvider } from '../context/AuthContext'
import SignUpPage from './SignUpPage'

const navigateMock = vi.fn()

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom')
  return {
    ...actual,
    useNavigate: () => navigateMock,
  }
})

function renderSignUpPage() {
  render(
    <MemoryRouter>
      <AuthProvider>
        <SignUpPage />
      </AuthProvider>
    </MemoryRouter>,
  )
}

async function fillForm(user, { displayName = 'Ada Shopper', email = 'ada@example.com', password = 's3cret-password!' } = {}) {
  if (displayName) await user.type(screen.getByLabelText('Name'), displayName)
  if (email) await user.type(screen.getByLabelText('Email'), email)
  if (password) await user.type(screen.getByLabelText('Password'), password)
}

describe('SignUpPage', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
    navigateMock.mockClear()
  })

  it('shows a validation error when required fields are missing', async () => {
    const user = userEvent.setup()
    renderSignUpPage()

    await user.click(screen.getByRole('button', { name: 'Sign up' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Name, email, and password are required.')
  })

  it('shows the server error when the email is already registered', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: false,
        json: () => Promise.resolve({ error: 'An account with this email already exists.' }),
      }),
    )

    const user = userEvent.setup()
    renderSignUpPage()
    await fillForm(user)

    await user.click(screen.getByRole('button', { name: 'Sign up' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('An account with this email already exists.')
  })

  it('redirects to / after a successful sign-up', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: () => Promise.resolve({ id: '1', email: 'ada@example.com', displayName: 'Ada Shopper' }),
      }),
    )

    const user = userEvent.setup()
    renderSignUpPage()
    await fillForm(user)

    await user.click(screen.getByRole('button', { name: 'Sign up' }))

    await waitFor(() => expect(navigateMock).toHaveBeenCalledWith('/'))
  })
})
