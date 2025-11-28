import { http } from './http'

export async function fetchVisits(params) {
  const { data } = await http.get('/visits', { params })
  return data
}

export async function createVisit(payload) {
  const { data } = await http.post('/visits', payload)
  return data
}

export async function updateVisit(id, payload) {
  const { data } = await http.put(`/visits/${id}`, payload)
  return data
}

export async function deleteVisit(id) {
  await http.delete(`/visits/${id}`)
}
