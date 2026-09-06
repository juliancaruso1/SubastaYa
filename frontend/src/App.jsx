import { NavLink, Route, Routes } from 'react-router-dom'
import { getCurrentUserId, setCurrentUserId } from './api/client'
import CatalogPage from './pages/CatalogPage'
import AuctionDetailPage from './pages/AuctionDetailPage'
import CreateAuctionPage from './pages/CreateAuctionPage'
import WalletPage from './pages/WalletPage'
import MyActivitiesPage from './pages/MyActivitiesPage'

const USUARIOS_DEMO = [
  { id: 2, label: 'comprador1@test.com' },
  { id: 3, label: 'comprador2@test.com' },
  { id: 4, label: 'sinfondos@test.com' },
  { id: 1, label: 'vendedor@test.com' }
]

export default function App() {
  return (
    <div className="app-shell">
      <nav className="top-nav">
        <NavLink to="/" className="brand">
          <span className="brand-mark">SubastaYa</span>
          <span className="brand-tag">tiempo real, sin sorpresas</span>
        </NavLink>

        <div className="nav-links">
          <NavLink to="/" end>Catálogo</NavLink>
          <NavLink to="/publicar">Publicar</NavLink>
          <NavLink to="/billetera">Billetera</NavLink>
          <NavLink to="/actividades">Mis actividades</NavLink>
        </div>

        <div className="user-switcher">
          <select defaultValue={getCurrentUserId()} onChange={(e) => { setCurrentUserId(Number(e.target.value)); window.location.reload() }}>
            {USUARIOS_DEMO.map((u) => <option key={u.id} value={u.id}>{u.label}</option>)}
          </select>
        </div>
      </nav>

      <Routes>
        <Route path="/" element={<CatalogPage />} />
        <Route path="/auctions/:id" element={<AuctionDetailPage />} />
        <Route path="/publicar" element={<CreateAuctionPage />} />
        <Route path="/billetera" element={<WalletPage />} />
        <Route path="/actividades" element={<MyActivitiesPage />} />
      </Routes>
    </div>
  )
}
