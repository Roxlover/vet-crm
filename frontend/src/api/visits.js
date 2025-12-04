import { http } from './http'

export async function fetchVisits(params) {
  const { data } = await http.get('/visits', { params })
  return data
}

export async function createVisit(payload) {
  const { data } = await http.post('/visits', payload)
  return data
}

export async function updateVisit(id, payload) {
  const { data } = await http.put(`/visits/${id}`, payload)
  return data
}

export async function deleteVisit(id) {
  await http.delete(`/visits/${id}`)
}
export async function uploadVisitImages(visitId, files) {
  const fd = new FormData()
  files.forEach(file => {
    fd.append('files', file)           // 🔹 backend [FromForm] List<IFormFile> files ile eşleşiyor
  })

  const res = await http.post(`/visits/${visitId}/images`, fd, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })

  return res.data                      // => List<VisitImageDto>
}
