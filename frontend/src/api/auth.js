import { http } from './http'
import { saveAuth, clearAuth } from '@/utils/auth'


export async function login(credentials) {
  console.log('[AUTH][LOGIN] payload >>>', credentials)

  const res = await http.post('/auth/login', credentials)

  console.log('[AUTH][LOGIN][RES]', res.status, res.data)

  saveAuth(res.data)

  return res.data
}

export function logout() {
  clearAuth()
}
