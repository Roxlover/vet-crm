import { http } from '@/api/http'

export async function fetchVisits(params) {
  const { data } = await http.get('/visits', { params })
  return data
}

export async function createVisit(payload) {
  const { data } = await http.post('/visits', payload)
  return data
}

export function updateVisitCollected(visitId, payload) {
  return http.patch(
    `/visits/${visitId}/collected`,
    {
      collectedAmountTl: payload.collectedAmountTl ?? null,
      note: payload.note ?? null
    },
    {
      headers: { "Content-Type": "application/json" }
    }
  );
}
export async function updateVisit(id, payload) {
  const { data } = await http.put(`/visits/${id}`, payload)
  return data
}

export async function updateVisitStatus(visitId, status) {
  await http.patch(`/visits/${visitId}/status`, { status })
}

export async function deleteVisit(id) {
  await http.delete(`/visits/${id}`)
}

export async function uploadVisitImages(visitId, files) {
  const fd = new FormData()

  for (const file of Array.from(files)) {
    fd.append('files', file) // backend expects 'files'
  }

  const { data } = await http.post(`/visits/${visitId}/images`, fd)

  return data
}
