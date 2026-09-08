export default function HealthBanner({ status = 'unknown' }) {
  return (
    <div role="status">
      <span>Backend status: </span>
      <strong>{status}</strong>
    </div>
  )
}
