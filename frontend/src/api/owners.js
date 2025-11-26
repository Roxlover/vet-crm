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

