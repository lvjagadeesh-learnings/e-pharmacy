import { Route, Routes, useNavigate } from 'react-router-dom'
import { useHealthStatus } from './api/useHealthStatus'
import HealthBanner from './components/HealthBanner'
import RequireAuth from './components/RequireAuth'
import { AuthProvider } from './context/AuthContext'
import { useAuth } from './context/useAuth'
import AccountPage from './pages/AccountPage'
import HomePage from './pages/HomePage'
import LoginPage from './pages/LoginPage'
import SignUpPage from './pages/SignUpPage'
import './App.css'

function AppHeader() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  async function handleLogout() {
    await logout()
    navigate('/')
  }

  return (
    <header id="app-header">
      <h1>e-Pharmacy</h1>
      <p>Your trusted online pharmacy</p>
      {user && (
        <button type="button" onClick={handleLogout}>
          Log out
        </button>
      )}
    </header>
  )
}

function App() {
  const healthStatus = useHealthStatus()

  return (
    <AuthProvider>
      <AppHeader />

      <main id="app-main">
        <HealthBanner status={healthStatus} />
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/signup" element={<SignUpPage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route
            path="/account"
            element={
              <RequireAuth>
                <AccountPage />
              </RequireAuth>
            }
          />
        </Routes>
      </main>
    </AuthProvider>
  )
}

export default App
