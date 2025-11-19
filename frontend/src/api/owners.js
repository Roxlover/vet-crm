import apiClient from './client'

export async function fetchOwners() {
  const res = await apiClient.get('/owners')
  return res.data
}

export async function createOwner(payload) {
  const res = await apiClient.post('/owners', payload)
  return res.data
}

export async function addPetToOwner(ownerId, petPayload) {
  const res = await apiClient.post(`/owners/${ownerId}/pets`, petPayload)
  return res.data
}

