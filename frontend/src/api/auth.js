import { http } from './http'
import { setAuth } from '../utils/auth'

export async function login({ username, password }) {
  try {
    const payload = { username, password }
    console.log('[LOGIN][REQ]', payload)

    const res = await http.post('/auth/login', payload)

    console.log('[LOGIN][RES]', res.status, res.data)

    const data = res.data

    const token = data.token
    const user =
      data.user ??
      {
        id: data.id,
        fullName: data.fullName,
        username: data.username,
        role: data.role,
      }

    if (!token || !user) {
      console.error('[LOGIN] Beklenen formatta değil:', data)
      throw new Error('Login response beklenen formatta değil')
    }

    setAuth(token, user)
    console.log('[LOGIN][OK] user:', user)
    return user
  } catch (err) {
    if (err.response) {
      console.log(
        '[LOGIN][ERR_RES]',
        err.response.status,
        err.response.data,
      )
    } else if (err.request) {
      console.log('[LOGIN][ERR_REQ_NO_RESPONSE]', err.message)
    } else {
      console.log('[LOGIN][ERR_OTHER]', err.message)
    }
    throw err
  }
}
