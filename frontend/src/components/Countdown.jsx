import { useEffect, useState } from 'react'

// Recibe la fecha de fin (ISO) y renderiza mm:ss, cambiando de estilo
// cuando entra en la ventana crítica (< 60s), tal como pide la consigna.
export default function Countdown({ fechaFin, className = '' }) {
  const [restanteMs, setRestanteMs] = useState(() => new Date(fechaFin) - new Date())

  useEffect(() => {
    const interval = setInterval(() => {
      setRestanteMs(new Date(fechaFin) - new Date())
    }, 1000)
    return () => clearInterval(interval)
  }, [fechaFin])

  if (restanteMs <= 0) {
    return <span className={`${className}`}>Finalizada</span>
  }

  const totalSegundos = Math.floor(restanteMs / 1000)
  const horas = Math.floor(totalSegundos / 3600)
  const minutos = Math.floor((totalSegundos % 3600) / 60)
  const segundos = totalSegundos % 60

  const critico = totalSegundos <= 60
  const advertencia = !critico && totalSegundos <= 300

  const texto = horas > 0
    ? `${horas}h ${minutos}m ${segundos}s`
    : `${String(minutos).padStart(2, '0')}:${String(segundos).padStart(2, '0')}`

  return (
    <span className={`${className} ${critico ? 'critical' : advertencia ? 'warning' : ''}`}>
      {texto}
    </span>
  )
}
