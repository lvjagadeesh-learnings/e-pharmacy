import { Link, Route, Routes, useNavigate } from 'react-router-dom'
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
import MembershipPage from './pages/MembershipPage'
import OrderConfirmationPage from './pages/OrderConfirmationPage'
import OrdersPage from './pages/OrdersPage'
import SignUpPage from './pages/SignUpPage'
import './App.css'

function CartIcon() {
  return (
    <svg
      aria-hidden="true"
      focusable="false"
      width="20"
      height="20"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <circle cx="9" cy="21" r="1" />
      <circle cx="20" cy="21" r="1" />
      <path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6" />
    </svg>
  )
}

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
      <div className="header-brand">
        <h1>
          <Link to="/">e-Pharmacy</Link>
        </h1>
        <p>Your trusted online pharmacy</p>
      </div>
      <div className="header-actions">
        <Link to="/membership">Membership</Link>
        {user ? (
          <>
            <Link to="/orders">Orders</Link>
            <Link
              to="/cart"
              data-testid="cart-badge"
              className="cart-link"
              aria-label={`Cart, ${itemCount} item(s)`}
            >
              <CartIcon />
              {itemCount > 0 && <span className="cart-count">{itemCount}</span>}
            </Link>
            <span className="header-username">{user.displayName}</span>
            <button type="button" onClick={handleLogout}>
              Log out
            </button>
          </>
        ) : (
          <>
            <Link to="/login">Log in</Link>
            <Link to="/signup">Sign up</Link>
          </>
        )}
      </div>
    </header>
  )
}

function App() {
  return (
    <AuthProvider>
      <CartProvider>
        <a href="#app-main" className="skip-link">
          Skip to main content
        </a>
        <AppHeader />

        <main id="app-main" tabIndex={-1}>
          <Routes>
            <Route path="/" element={<CatalogPage />} />
            <Route path="/medicines/:medicineId" element={<MedicineDetailPage />} />
            <Route path="/signup" element={<SignUpPage />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/membership" element={<MembershipPage />} />
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
