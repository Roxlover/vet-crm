import apiClient from './client'

export async function createVisit(payload) {
  const response = await apiClient.post('/visits', payload)
  return response.data
}
