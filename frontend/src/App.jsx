import { Route, Routes } from 'react-router-dom'
import { useHealthStatus } from './api/useHealthStatus'
import HealthBanner from './components/HealthBanner'
import { AuthProvider } from './context/AuthContext'
import HomePage from './pages/HomePage'
import SignUpPage from './pages/SignUpPage'
import './App.css'

function App() {
  const healthStatus = useHealthStatus()

  return (
    <AuthProvider>
      <header id="app-header">
        <h1>e-Pharmacy</h1>
        <p>Your trusted online pharmacy</p>
      </header>

      <main id="app-main">
        <HealthBanner status={healthStatus} />
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/signup" element={<SignUpPage />} />
        </Routes>
      </main>
    </AuthProvider>
  )
}

export default App
