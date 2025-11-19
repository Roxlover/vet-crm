import apiClient from './client'

export async function fetchPetsByOwner(ownerId) {
  const res = await apiClient.get('/pets', {
    params: { ownerId }
  })
  return res.data
}
