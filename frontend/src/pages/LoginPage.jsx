import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../context/useAuth'

function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')

    if (!email.trim() || !password.trim()) {
      setError('Email and password are required.')
      return
    }

    try {
      await login({ email, password })
      navigate('/')
    } catch (err) {
      setError(err.message)
    }
  }

  return (
    <div className="page-container">
      <h2>Log in</h2>
      <form onSubmit={handleSubmit}>
        <div className="field">
          <label className="field__label" htmlFor="email">
            Email
          </label>
          <input
            className="field__input"
            id="email"
            name="email"
            type="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
          />
        </div>

        <div className="field">
          <label className="field__label" htmlFor="password">
            Password
          </label>
          <input
            className="field__input"
            id="password"
            name="password"
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />
        </div>

        {error && (
          <p role="alert" aria-live="polite" className="form-error">
            {error}
          </p>
        )}

        <button type="submit" className="button button--primary">
          Log in
        </button>
      </form>
    </div>
  )
}

export default LoginPage
