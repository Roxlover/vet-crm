import { http } from '@/api/http'

// ÖZET (Bugün / Yarın / Geciken / Yapıldı kutuları)
export async function fetchReminderSummary() {
  const response = await http.get('/dashboard/reminders-summary')
  return response.data
}

// Filtreye göre liste (today / tomorrow / overdue / upcoming / done)
export async function fetchReminders(filter) {
  const response = await http.get('/dashboard/reminders', {
    params: { filter },
  })
  return response.data
}

// Ziyaret detayı (modal)
// export async function fetchVisitDetail(visitId) {
//   const res = await http.get(`/dashboard/visit/${visitId}`)
//   return res.data
// }
// Takvim için randevu listesi
export async function fetchCalendarAppointments(from, to) {
  const { data } = await http.get('/calendar/appointments', {
    params: { from, to },
  })
  return data
}

// Bir hasta sahibine ait diğer hayvanlar
export async function fetchOwnerPets(ownerId) {
  const { data } = await http.get(`/owners/${ownerId}/pets`)
  return data
}

// Yeni randevu oluştur
export async function createAppointment(payload) {
  const { data } = await http.post('/appointments', payload)
  return data
}

// Doktor listesi
export async function fetchDoctors() {
  const { data } = await http.get('/doctors')
  return data
}

export async function updateReminderStatus(id, completed, markAsOverdue = false) {
  try {
    await http.patch(`/reminders/${id}/status`, {
      completed,
      markAsOverdue,
    })
  } catch (err) {
    console.error('[REMINDERS][UPDATE_STATUS_ERR]', err)
    throw err    // veya burada swallow edip UI'da toast göster
  }
}
export function fetchVisitDetail(visitId) {
  return http.get(`/visits/${visitId}`).then(res => res.data)
}



export async function searchOwners(query) {
  const { data } = await http.get('/owners/search', {
    params: { query },
  })
  return data
}