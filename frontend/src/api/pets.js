import { http } from './http'

export async function fetchPetsByOwner(ownerId) {
  const res = await http.get('/pets', {
    params: { ownerId }
  })
  return res.data
}
