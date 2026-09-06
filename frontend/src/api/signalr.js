import * as signalR from '@microsoft/signalr'

const HUB_URL = 'http://localhost:5000/hubs/auctions'

export function crearConexionSubasta(subastaId, { onPujaRegistrada, onSubastaCerrada }) {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL)
    .withAutomaticReconnect()
    .build()

  connection.on('PujaRegistrada', (detalle) => onPujaRegistrada?.(detalle))
  connection.on('SubastaCerrada', (payload) => onSubastaCerrada?.(payload))

  connection.start()
    .then(() => connection.invoke('UnirseASubasta', subastaId))
    .catch((err) => console.error('No se pudo conectar a SignalR', err))

  return connection
}

export async function cerrarConexion(connection, subastaId) {
  if (!connection) return
  try {
    await connection.invoke('SalirDeSubasta', subastaId)
  } catch {
    // la conexión puede haberse cerrado ya; no es un error crítico
  }
  await connection.stop()
}
