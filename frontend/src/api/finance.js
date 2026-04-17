import { http } from '@/api/http'

export function fetchMyFinanceSummary() {
  return http.get('/finance/me/summary').then(r => r.data)
}

export function fetchMyFinanceEntries(take = 100) {
  return http.get('/finance/me/entries', { params: { take } }).then(r => r.data)
}

