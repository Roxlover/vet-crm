import apiClient from './client'

export async function fetchReminderSummary() {
  const response = await apiClient.get('/dashboard/reminders-summary')
  return response.data
}

export async function fetchReminders(filter) {
  const response = await apiClient.get('/dashboard/reminders', {
    params: { filter },
  })
  return response.data
}

// export async function fetchVisitDetail(id) {
//   const res = await apiClient.get(`/visits/${id}`)
//   return res.data
// }

export async function fetchVisitDetail(visitId) {
  const res = await apiClient.get(`/dashboard/visit/${visitId}`)
  return res.data
}