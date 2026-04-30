import { http } from './http'


export async function fetchOwners() {
  const res = await http.get('/owners')
  return res.data
}

export async function createOwner(payload) {
  const res = await http.post('/owners', payload)
  return res.data
}

export async function addPetToOwner(ownerId, petPayload) {
  const res = await http.post(`/owners/${ownerId}/pets`, petPayload)
  return res.data
}

export async function fetchOwner(ownerId) {
  const res = await http.get(`/owners/${ownerId}`)
  return res.data
}

export async function deletePet(petId) {
  const res = await http.delete(`/pets/${petId}`)
  return res.data
}

export async function addOwnerNote(ownerId, note) {
  const res = await http.post(`/owners/${ownerId}/notes`, { note })
  return res.data
}

export async function searchOwners(query) {
  const res = await http.get('/owners/search', { params: { query } })
  return res.data
}
