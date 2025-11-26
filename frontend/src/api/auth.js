// src/api/auth.js
import { http } from './http'
import { setAuth } from '../utils/auth'

export async function login({ username, password }) {
  const res = await http.post('/auth/login', { username, password })

  const data = res.data
  console.log('[LOGIN] response data >>', data)

  // İki olası formatı da destekleyelim:
  // 1) { token, user: { id, fullName, username, role } }
  // 2) { token, id, fullName, username, role }
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
    throw new Error('Login response beklenen formatta değil')
  }

  // token + user bilgisini localStorage’a yaz
  setAuth(token, user)

  // Vue tarafı için user objesini geri döndürüyoruz
  return user
}
