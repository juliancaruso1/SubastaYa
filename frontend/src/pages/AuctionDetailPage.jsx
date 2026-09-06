import { useEffect, useMemo, useRef, useState } from 'react'
import { useParams } from 'react-router-dom'
import { api, getCurrentUserId } from '../api/client'
import { crearConexionSubasta, cerrarConexion } from '../api/signalr'
import Countdown from '../components/Countdown'
import { useToast } from '../components/ToastProvider'

export default function AuctionDetailPage() {
  const { id } = useParams()
  const { pushToast } = useToast()
  const userId = getCurrentUserId()

  const [subasta, setSubasta] = useState(null)
  const [montoPersonalizado, setMontoPersonalizado] = useState('')
  const [enviando, setEnviando] = useState(false)
  const connectionRef = useRef(null)

  const cargarDetalle = () => {
    api.get(`/auctions/${id}`).then((res) => setSubasta(res.data))
  }

  useEffect(() => {
    cargarDetalle()

    connectionRef.current = crearConexionSubasta(Number(id), {
      onPujaRegistrada: (detalle) => {
        setSubasta(detalle)
        pushToast(`Nueva oferta: $${detalle.pujaActual.toLocaleString('es-AR')}`, 'info')
      },
      onSubastaCerrada: () => {
        pushToast('La subasta finalizó. Se liquidaron los fondos.', 'success')
        cargarDetalle()
      }
    })

    return () => { cerrarConexion(connectionRef.current, Number(id)) }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id])

  // Indicador "Liderando" / "Superado (Outbid)" comparando al postor líder
  // actual contra el usuario simulado activo en esta sesión.
  const indicadorLider = useMemo(() => {
    if (!subasta || subasta.historialOfertas.length === 0) return null
    const lider = subasta.historialOfertas[0]
    if (lider.compradorId === userId) return 'leading'
    const yoParticipe = subasta.historialOfertas.some((p) => p.compradorId === userId)
    return yoParticipe ? 'outbid' : null
  }, [subasta, userId])

  if (!subasta) return <p className="empty-state">Cargando sala de subasta…</p>

  const sugerido = subasta.proximaPujaSugerida

  const enviarPuja = async (monto) => {
    if (enviando) return
    setEnviando(true)
    try {
      await api.post(`/auctions/${id}/bids`, {
        compradorId: userId,
        monto,
        versionEsperada: subasta.version
      })
      pushToast('Puja confirmada.', 'success')
      setMontoPersonalizado('')
    } catch (err) {
      const status = err?.response?.status
      const mensaje = err?.response?.data?.mensaje
      if (status === 422) pushToast(mensaje || 'Fondos insuficientes para esta puja.', 'error')
      else if (status === 409) {
        pushToast('La subasta cambió mientras ofertabas. Actualizando…', 'error')
        cargarDetalle()
      }
      else pushToast(mensaje || 'No se pudo registrar la puja.', 'error')
    } finally {
      setEnviando(false)
    }
  }

  return (
    <div>
      <div className="page-header">
        <span className={`badge estado-${subasta.estado}`}>{subasta.estado}</span>
        <h1 style={{ marginTop: 10 }}>{subasta.titulo}</h1>
        <p className="page-subtitle">{subasta.categoria}</p>
      </div>

      <div className="live-room">
        <div>
          <div className="live-hero">
            <img src={subasta.urlImagen} alt={subasta.titulo} />
          </div>
          <p style={{ marginTop: 16, color: 'var(--text-muted)' }}>{subasta.descripcion}</p>

          <h2 style={{ marginTop: 28 }}>Historial de ofertas</h2>
          <div className="bid-history">
            {subasta.historialOfertas.length === 0 && (
              <p className="empty-state">Todavía no hay ofertas. ¡Sé el primero!</p>
            )}
            {subasta.historialOfertas.map((p) => (
              <div key={p.id} className="bid-row">
                <span>{p.compradorSeudonimo}</span>
                <span className="monto">${p.monto.toLocaleString('es-AR')}</span>
                <span className="hora">{new Date(p.fechaPuja).toLocaleTimeString('es-AR')}</span>
              </div>
            ))}
          </div>
        </div>

        <div className="live-panel">
          <div>
            <span className="price-label">Tiempo restante</span>
            <Countdown fechaFin={subasta.fechaFin} className="timer-display" />
          </div>

          <div>
            <span className="price-label">Puja actual</span>
            <div className="price-tag" style={{ fontSize: 26 }}>
              ${subasta.pujaActual.toLocaleString('es-AR')}
            </div>
          </div>

          {indicadorLider && (
            <span className={`status-pill ${indicadorLider}`}>
              {indicadorLider === 'leading' ? 'Liderando' : 'Superado (Outbid)'}
            </span>
          )}

          {subasta.estado === 'Activa' ? (
            <div className="bid-console">
              <button
                className="btn-primary"
                disabled={enviando}
                onClick={() => enviarPuja(sugerido)}
              >
                Ofertar ${sugerido.toLocaleString('es-AR')}
              </button>

              <div style={{ display: 'flex', gap: 8 }}>
                <input
                  type="number"
                  placeholder={`Monto personalizado (mín. ${sugerido})`}
                  value={montoPersonalizado}
                  onChange={(e) => setMontoPersonalizado(e.target.value)}
                />
                <button
                  className="btn-secondary"
                  disabled={enviando || !montoPersonalizado}
                  onClick={() => enviarPuja(Number(montoPersonalizado))}
                >
                  Ofertar
                </button>
              </div>
            </div>
          ) : (
            <p className="page-subtitle">Esta subasta no admite más ofertas ({subasta.estado}).</p>
          )}
        </div>
      </div>
    </div>
  )
}
