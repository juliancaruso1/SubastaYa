import { Link } from 'react-router-dom'
import Countdown from './Countdown'

export default function AuctionCard({ subasta }) {
  return (
    <Link to={`/auctions/${subasta.id}`} className="auction-card">
      <img src={subasta.urlImagen} alt={subasta.titulo} loading="lazy" />
      <div className="auction-card-body">
        <span className="auction-card-category">{subasta.categoria}</span>
        <span className="auction-card-title">{subasta.titulo}</span>
        <span className={`badge estado-${subasta.estado}`}>{subasta.estado}</span>
        <div className="auction-card-footer">
          <div>
            <span className="price-label">{subasta.cantidadPujas} ofertas</span>
            <span className="price-tag">${subasta.pujaActual.toLocaleString('es-AR')}</span>
          </div>
          <Countdown fechaFin={subasta.fechaFin} className="countdown" />
        </div>
      </div>
    </Link>
  )
}
