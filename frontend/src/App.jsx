import { Link, Route, Routes, useNavigate } from 'react-router-dom'
import { useHealthStatus } from './api/useHealthStatus'
import HealthBanner from './components/HealthBanner'
import RequireAuth from './components/RequireAuth'
import { AuthProvider } from './context/AuthContext'
import { CartProvider } from './context/CartContext'
import { useAuth } from './context/useAuth'
import { useCart } from './context/useCart'
import AccountPage from './pages/AccountPage'
import CartPage from './pages/CartPage'
import CatalogPage from './pages/CatalogPage'
import CheckoutPage from './pages/CheckoutPage'
import LoginPage from './pages/LoginPage'
import MedicineDetailPage from './pages/MedicineDetailPage'
import OrderConfirmationPage from './pages/OrderConfirmationPage'
import OrdersPage from './pages/OrdersPage'
import SignUpPage from './pages/SignUpPage'
import './App.css'

function AppHeader() {
  const { user, logout } = useAuth()
  const { itemCount } = useCart()
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
        <>
          <Link to="/orders">Orders</Link>
          <Link to="/cart" data-testid="cart-badge">
            Cart ({itemCount})
          </Link>
          <button type="button" onClick={handleLogout}>
            Log out
          </button>
        </>
      )}
    </header>
  )
}

function App() {
  const healthStatus = useHealthStatus()

  return (
    <AuthProvider>
      <CartProvider>
        <a href="#app-main" className="skip-link">
          Skip to main content
        </a>
        <AppHeader />

        <main id="app-main" tabIndex={-1}>
          <HealthBanner status={healthStatus} />
          <Routes>
            <Route path="/" element={<CatalogPage />} />
            <Route path="/medicines/:medicineId" element={<MedicineDetailPage />} />
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
            <Route
              path="/cart"
              element={
                <RequireAuth>
                  <CartPage />
                </RequireAuth>
              }
            />
            <Route
              path="/checkout"
              element={
                <RequireAuth>
                  <CheckoutPage />
                </RequireAuth>
              }
            />
            <Route
              path="/orders"
              element={
                <RequireAuth>
                  <OrdersPage />
                </RequireAuth>
              }
            />
            <Route
              path="/orders/:orderId"
              element={
                <RequireAuth>
                  <OrderConfirmationPage />
                </RequireAuth>
              }
            />
          </Routes>
        </main>
      </CartProvider>
    </AuthProvider>
  )
}

export default App
