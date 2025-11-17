import apiClient from './client'

export async function fetchOwners() {
  const response = await apiClient.get('/owners')
  return response.data
}

export async function createOwner(payload) {
  const response = await apiClient.post('/owners', payload)
  return response.data
}
