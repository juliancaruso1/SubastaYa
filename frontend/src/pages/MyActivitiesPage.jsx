import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api, getCurrentUserId } from '../api/client'

export default function MyActivitiesPage() {
  const userId = getCurrentUserId()
  const [tab, setTab] = useState('compras')
  const [pujas, setPujas] = useState([])
  const [publicaciones, setPublicaciones] = useState([])

  useEffect(() => {
    api.get(`/users/${userId}/bids`).then((res) => setPujas(res.data))
    api.get(`/users/${userId}/auctions`).then((res) => setPublicaciones(res.data))
  }, [userId])

  return (
    <div>
      <div className="page-header">
        <h1>Mis actividades</h1>
        <p className="page-subtitle">Seguimiento de tus compras y publicaciones.</p>
      </div>

      <div className="tabs">
        <button className={tab === 'compras' ? 'active' : ''} onClick={() => setTab('compras')}>Mis Compras / Pujas</button>
        <button className={tab === 'publicaciones' ? 'active' : ''} onClick={() => setTab('publicaciones')}>Mis Publicaciones</button>
      </div>

      {tab === 'compras' && (
        pujas.length === 0 ? <p className="empty-state">Todavía no participaste de ninguna subasta.</p> : (
          <table>
            <thead><tr><th>Subasta</th><th>Mi mejor oferta</th><th>Puja actual</th><th>Estado</th></tr></thead>
            <tbody>
              {pujas.map((p) => (
                <tr key={p.subastaId}>
                  <td><Link to={`/auctions/${p.subastaId}`}>{p.titulo}</Link></td>
                  <td className="monto">${p.miMejorOferta.toLocaleString('es-AR')}</td>
                  <td className="monto">${p.pujaActual.toLocaleString('es-AR')}</td>
                  <td>{p.gano ? 'Ganada 🏆' : p.estado === 'Activa' ? 'En curso' : p.estado}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )
      )}

      {tab === 'publicaciones' && (
        publicaciones.length === 0 ? <p className="empty-state">Todavía no publicaste ninguna subasta.</p> : (
          <table>
            <thead><tr><th>Subasta</th><th>Ofertas recibidas</th><th>Recaudación</th><th>Estado</th></tr></thead>
            <tbody>
              {publicaciones.map((p) => (
                <tr key={p.id}>
                  <td><Link to={`/auctions/${p.id}`}>{p.titulo}</Link></td>
                  <td>{p.cantidadPujas}</td>
                  <td className="monto">{p.recaudacion ? `$${p.recaudacion.toLocaleString('es-AR')}` : '—'}</td>
                  <td>{p.estado}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )
      )}
    </div>
  )
}
