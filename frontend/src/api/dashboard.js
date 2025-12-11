import { http } from '@/api/http'

// ÖZET (Bugün / Yarın / Geciken / Yapıldı kutuları)
export async function fetchReminderSummary() {
  const { data } = await http.get('/dashboard/reminders-summary')
  return data
}

// Filtreye göre liste (today / tomorrow / overdue / upcoming / done)
export async function fetchReminders(filter) {
  const { data } = await http.get('/dashboard/reminders', {
    params: { filter },
  })
  return data
}

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

export async function createAppointment(payload) {
  const { data } = await http.post('/appointments', payload)
  return data
}

export async function fetchDoctors() {
  const { data } = await http.get('/doctors')
  return data
}

export async function updateReminderStatus(id, completed, markAsOverdue = false) {
  const { data } = await http.post(`/reminders/${id}/status`, {
    completed,
    markAsOverdue,
  })
  return data
}

export function fetchVisitDetail(visitId) {
  return http.get(`/visits/${visitId}`).then((res) => res.data)
}

export async function searchOwners(query) {
  const { data } = await http.get('/owners/search', {
    params: { query },
  })
  return data
}
