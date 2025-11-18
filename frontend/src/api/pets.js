import apiClient from './client'

export async function fetchPets() {
  const response = await apiClient.get('/pets')
  return response.data
}
