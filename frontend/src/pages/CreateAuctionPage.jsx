import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api, getCurrentUserId } from '../api/client'
import { useToast } from '../components/ToastProvider'

const ESTADO_INICIAL = {
  titulo: '', descripcion: '', urlImagen: '', categoriaId: '',
  precioBase: '', incrementoMinimo: '', fechaInicio: '', fechaFin: ''
}

export default function CreateAuctionPage() {
  const navigate = useNavigate()
  const { pushToast } = useToast()
  const userId = getCurrentUserId()

  const [form, setForm] = useState(ESTADO_INICIAL)
  const [categorias, setCategorias] = useState([])
  const [errores, setErrores] = useState({})
  const [enviando, setEnviando] = useState(false)

  useEffect(() => {
    api.get('/categories').then((res) => setCategorias(res.data))
  }, [])

  const set = (campo) => (e) => setForm({ ...form, [campo]: e.target.value })

  const validar = () => {
    const e = {}
    if (!form.titulo.trim()) e.titulo = 'El título es obligatorio.'
    if (!form.categoriaId) e.categoriaId = 'Elegí una categoría.'
    if (!form.precioBase || Number(form.precioBase) <= 0) e.precioBase = 'Debe ser un valor positivo.'
    if (!form.incrementoMinimo || Number(form.incrementoMinimo) <= 0) e.incrementoMinimo = 'Debe ser un valor positivo.'
    if (!form.fechaInicio) e.fechaInicio = 'Requerido.'
    if (!form.fechaFin) e.fechaFin = 'Requerido.'
    if (form.fechaInicio && form.fechaFin && new Date(form.fechaFin) <= new Date(form.fechaInicio)) {
      e.fechaFin = 'La fecha de finalización debe ser posterior a la de inicio.'
    }
    setErrores(e)
    return Object.keys(e).length === 0
  }

  const enviar = async (ev) => {
    ev.preventDefault()
    if (!validar()) return

    setEnviando(true)
    try {
      const res = await api.post('/auctions', {
        vendedorId: userId,
        titulo: form.titulo,
        descripcion: form.descripcion,
        urlImagen: form.urlImagen || 'https://picsum.photos/seed/nueva/400/300',
        categoriaId: Number(form.categoriaId),
        precioBase: Number(form.precioBase),
        incrementoMinimo: Number(form.incrementoMinimo),
        fechaInicio: new Date(form.fechaInicio).toISOString(),
        fechaFin: new Date(form.fechaFin).toISOString()
      })
      pushToast('Subasta publicada con éxito.', 'success')
      navigate(`/auctions/${res.data.id}`)
    } catch (err) {
      pushToast(err?.response?.data?.mensaje || 'No se pudo publicar la subasta.', 'error')
    } finally {
      setEnviando(false)
    }
  }

  return (
    <div>
      <div className="page-header">
        <h1>Publicar una subasta</h1>
        <p className="page-subtitle">Completá los datos del producto y la ventana temporal.</p>
      </div>

      <form className="form-card" onSubmit={enviar}>
        <div className="field">
          <label>Título</label>
          <input value={form.titulo} onChange={set('titulo')} placeholder="Ej. Notebook Gamer RTX" />
          {errores.titulo && <span className="field-error">{errores.titulo}</span>}
        </div>

        <div className="field">
          <label>Descripción</label>
          <textarea rows={3} value={form.descripcion} onChange={set('descripcion')} />
        </div>

        <div className="field">
          <label>URL de imagen</label>
          <input value={form.urlImagen} onChange={set('urlImagen')} placeholder="https://..." />
        </div>

        <div className="field">
          <label>Categoría</label>
          <select value={form.categoriaId} onChange={set('categoriaId')}>
            <option value="">Elegí una categoría</option>
            {categorias.map((c) => <option key={c.id} value={c.id}>{c.nombre}</option>)}
          </select>
          {errores.categoriaId && <span className="field-error">{errores.categoriaId}</span>}
        </div>

        <div className="field-row">
          <div className="field">
            <label>Precio base</label>
            <input type="number" value={form.precioBase} onChange={set('precioBase')} />
            {errores.precioBase && <span className="field-error">{errores.precioBase}</span>}
          </div>
          <div className="field">
            <label>Incremento mínimo</label>
            <input type="number" value={form.incrementoMinimo} onChange={set('incrementoMinimo')} />
            {errores.incrementoMinimo && <span className="field-error">{errores.incrementoMinimo}</span>}
          </div>
        </div>

        <div className="field-row">
          <div className="field">
            <label>Fecha/hora de inicio</label>
            <input type="datetime-local" value={form.fechaInicio} onChange={set('fechaInicio')} />
            {errores.fechaInicio && <span className="field-error">{errores.fechaInicio}</span>}
          </div>
          <div className="field">
            <label>Fecha/hora de finalización</label>
            <input type="datetime-local" value={form.fechaFin} onChange={set('fechaFin')} />
            {errores.fechaFin && <span className="field-error">{errores.fechaFin}</span>}
          </div>
        </div>

        <button className="btn-primary" type="submit" disabled={enviando}>
          {enviando ? 'Publicando…' : 'Publicar subasta'}
        </button>
      </form>
    </div>
  )
}
