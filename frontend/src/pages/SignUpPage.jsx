import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../context/useAuth'

function SignUpPage() {
  const { register } = useAuth()
  const navigate = useNavigate()
  const [displayName, setDisplayName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')

    if (!displayName.trim() || !email.trim() || !password.trim()) {
      setError('Name, email, and password are required.')
      return
    }

    if (!email.includes('@')) {
      setError('Please enter a valid email address.')
      return
    }

    try {
      await register({ email, password, displayName })
      navigate('/')
    } catch (err) {
      setError(err.message)
    }
  }

  return (
    <div className="page-container">
      <h2>Sign up</h2>
      <form onSubmit={handleSubmit}>
        <div className="field">
          <label className="field__label" htmlFor="displayName">
            Name
          </label>
          <input
            className="field__input"
            id="displayName"
            name="displayName"
            type="text"
            value={displayName}
            onChange={(event) => setDisplayName(event.target.value)}
          />
        </div>

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
          Sign up
        </button>
      </form>
    </div>
  )
}

export default SignUpPage
