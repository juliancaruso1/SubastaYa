import axios from 'axios'

export const API_BASE_URL = 'http://localhost:5000/api/v1'

export const api = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' }
})

// Usuario "logueado" simulado (no hay auth real en el alcance del TP).
// Se persiste en memoria durante la sesión del navegador.
export const CURRENT_USER_ID_KEY = 'subastaya_current_user_id'

export function getCurrentUserId() {
  const stored = sessionStorage_safe_get(CURRENT_USER_ID_KEY)
  return stored ? Number(stored) : 2 // comprador1@test.com por defecto
}

export function setCurrentUserId(id) {
  sessionStorage_safe_set(CURRENT_USER_ID_KEY, String(id))
}

// Wrapper defensivo: en el sandbox de artifacts no hay sessionStorage,
// pero esta app corre standalone con Vite, donde sí está disponible.
function sessionStorage_safe_get(key) {
  try { return window.sessionStorage.getItem(key) } catch { return null }
}
function sessionStorage_safe_set(key, value) {
  try { window.sessionStorage.setItem(key, value) } catch { /* no-op */ }
}
