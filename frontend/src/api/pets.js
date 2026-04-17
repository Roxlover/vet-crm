import { http } from '@/api/http'

// 🔹 TÜM PETLER (opsiyonel ownerId)
export async function fetchPets(ownerId) {
  const params = ownerId ? { ownerId } : undefined
  const res = await http.get('/pets', { params })
  return res.data
}

// 🔹 VisitsView.vue geriye dönük uyumluluk
export async function fetchPetsByOwner(ownerId) {
  const res = await http.get('/pets', {
    params: { ownerId },
  })
  return res.data
}

// 🔹 PET PROFİL (ziyaretler + görseller)
export async function fetchPetProfile(petId) {
  const res = await http.get(`/pets/${petId}/profile`)
  return res.data
}
