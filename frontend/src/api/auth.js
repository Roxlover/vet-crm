// src/api/auth.js

import { http } from './http'
import { saveAuth, clearAuth } from '@/utils/auth'

/**
 * Login – backend: POST /api/auth/login
 * body: { username, password }
 */
export async function login(credentials) {
  console.log('[AUTH][LOGIN] payload >>>', credentials)

  const res = await http.post('/auth/login', credentials)

  console.log('[AUTH][LOGIN][RES]', res.status, res.data)

  // token + user bilgisini kaydet
  saveAuth(res.data)

  return res.data
}

export function logout() {
  clearAuth()
}
