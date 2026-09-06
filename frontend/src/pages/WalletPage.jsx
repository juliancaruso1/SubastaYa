import { useEffect, useState } from 'react'
import { api, getCurrentUserId } from '../api/client'
import { useToast } from '../components/ToastProvider'

const ETIQUETAS_TIPO = {
  Deposito: 'Depósito',
  Retencion: 'Retención (garantía)',
  Liberacion: 'Liberación',
  Pago: 'Pago (subasta ganada)',
  Cobro: 'Cobro (venta)'
}

export default function WalletPage() {
  const userId = getCurrentUserId()
  const { pushToast } = useToast()
  const [saldo, setSaldo] = useState(null)
  const [movimientos, setMovimientos] = useState([])
  const [montoCarga, setMontoCarga] = useState('')

  const cargarTodo = () => {
    api.get('/wallet/balance', { params: { usuarioId: userId } }).then((res) => setSaldo(res.data))
    api.get('/wallet/movements', { params: { usuarioId: userId } }).then((res) => setMovimientos(res.data))
  }

  useEffect(cargarTodo, [userId])

  const acreditar = async () => {
    const monto = Number(montoCarga)
    if (!monto || monto <= 0) return
    try {
      await api.post('/wallet/deposits', { usuarioId: userId, monto })
      pushToast(`Se acreditaron $${monto.toLocaleString('es-AR')} a tu billetera.`, 'success')
      setMontoCarga('')
      cargarTodo()
    } catch (err) {
      pushToast(err?.response?.data?.mensaje || 'No se pudo acreditar el saldo.', 'error')
    }
  }

  if (!saldo) return <p className="empty-state">Cargando billetera…</p>

  return (
    <div>
      <div className="page-header">
        <h1>Billetera Virtual</h1>
        <p className="page-subtitle">Tus fondos respaldan cada puja en garantía (escrow).</p>
      </div>

      <div className="wallet-stats">
        <div className="stat-card">
          <div className="stat-label">Saldo total</div>
          <div className="stat-value">${saldo.saldoTotal.toLocaleString('es-AR')}</div>
        </div>
        <div className="stat-card retenido">
          <div className="stat-label">Retenido en garantía</div>
          <div className="stat-value">${saldo.saldoRetenido.toLocaleString('es-AR')}</div>
        </div>
        <div className="stat-card disponible">
          <div className="stat-label">Disponible</div>
          <div className="stat-value">${saldo.saldoDisponible.toLocaleString('es-AR')}</div>
        </div>
      </div>

      <h2>Carga de saldo simulada</h2>
      <div className="deposit-form">
        <input
          type="number"
          placeholder="Monto a acreditar"
          value={montoCarga}
          onChange={(e) => setMontoCarga(e.target.value)}
        />
        <button className="btn-primary" onClick={acreditar}>Acreditar saldo</button>
      </div>

      <h2>Historial de movimientos</h2>
      {movimientos.length === 0 ? (
        <p className="empty-state">Todavía no hay movimientos.</p>
      ) : (
        <table>
          <thead>
            <tr><th>Tipo</th><th>Monto</th><th>Fecha</th></tr>
          </thead>
          <tbody>
            {movimientos.map((m, idx) => (
              <tr key={idx}>
                <td>{ETIQUETAS_TIPO[m.tipo] || m.tipo}</td>
                <td className="monto">${m.monto.toLocaleString('es-AR')}</td>
                <td>{new Date(m.fecha).toLocaleString('es-AR')}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}
