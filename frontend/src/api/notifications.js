import { http } from './http'

export async function fetchNotifications() {
  const res = await http.get('/notifications')
  return res.data
}

export async function markNotificationsRead() {
  await http.post('/notifications/read')
}
