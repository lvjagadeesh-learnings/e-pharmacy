import { useAuth } from '../context/useAuth'

function AccountPage() {
  const { user } = useAuth()

  return (
    <div>
      <h2>Your account</h2>
      <p>Welcome, {user?.displayName ?? user?.email}.</p>
    </div>
  )
}

export default AccountPage
