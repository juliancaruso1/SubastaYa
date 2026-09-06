import { useEffect, useState } from 'react'
import { api } from '../api/client'
import AuctionCard from '../components/AuctionCard'

export default function CatalogPage() {
  const [subastas, setSubastas] = useState([])
  const [categorias, setCategorias] = useState([])
  const [loading, setLoading] = useState(true)
  const [filtros, setFiltros] = useState({ estado: '', categoriaId: '', orden: 'tiempo_restante' })

  useEffect(() => {
    api.get('/categories').then((res) => setCategorias(res.data)).catch(() => {})
  }, [])

  useEffect(() => {
    setLoading(true)
    const params = {}
    if (filtros.estado) params.estado = filtros.estado
    if (filtros.categoriaId) params.categoriaId = filtros.categoriaId
    if (filtros.orden) params.orden = filtros.orden

    api.get('/auctions', { params })
      .then((res) => setSubastas(res.data))
      .finally(() => setLoading(false))
  }, [filtros])

  return (
    <div>
      <div className="page-header">
        <h1>Subastas</h1>
        <p className="page-subtitle">Explorá lo que está en juego ahora mismo.</p>
      </div>

      <div className="filters-bar">
        <select value={filtros.estado} onChange={(e) => setFiltros({ ...filtros, estado: e.target.value })}>
          <option value="">Todos los estados</option>
          <option value="Activa">Activas</option>
          <option value="Programada">Próximas</option>
          <option value="Finalizada">Finalizadas</option>
        </select>

        <select value={filtros.categoriaId} onChange={(e) => setFiltros({ ...filtros, categoriaId: e.target.value })}>
          <option value="">Todas las categorías</option>
          {categorias.map((c) => (
            <option key={c.id} value={c.id}>{c.nombre}</option>
          ))}
        </select>

        <select value={filtros.orden} onChange={(e) => setFiltros({ ...filtros, orden: e.target.value })}>
          <option value="tiempo_restante">Menor tiempo restante</option>
          <option value="mayor_puja">Mayor puja</option>
        </select>
      </div>

      {loading ? (
        <p className="empty-state">Cargando subastas…</p>
      ) : subastas.length === 0 ? (
        <p className="empty-state">No hay subastas que coincidan con estos filtros.</p>
      ) : (
        <div className="auction-grid">
          {subastas.map((s) => <AuctionCard key={s.id} subasta={s} />)}
        </div>
      )}
    </div>
  )
}
