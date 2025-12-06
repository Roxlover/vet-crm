// src/utils/auth.js

const TOKEN_KEY = 'vetcrm_token'
const USER_KEY = 'vetcrm_user'

export function saveAuth(auth) {
  if (!auth) {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
    return
  }

  if (auth.token) {
    localStorage.setItem(TOKEN_KEY, auth.token)
  }

  localStorage.setItem(USER_KEY, JSON.stringify(auth))
}

export function getToken() {
  return localStorage.getItem(TOKEN_KEY) || null
}

export function getUser() {
  const raw = localStorage.getItem(USER_KEY)
  if (!raw) return null
  try {
    return JSON.parse(raw)
  } catch {
    return null
  }
}

export function clearAuth() {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(USER_KEY)
}
