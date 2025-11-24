// frontend/src/api/ledger.js
import { http } from '@/api/http'

// Günlük / haftalık / aylık kayıtları çeken fonksiyon
export async function fetchLedgerRange(from, to) {
  console.log('LEDGER RANGE PARAMS >>>', { from, to })

  try {
    const { data } = await http.get('/ledger', {
      params: { from, to },
    })
    console.log('LEDGER RANGE OK >>>', data)
    return data
  } catch (err) {
    console.error('LEDGER RANGE ERROR RAW >>>', err)
    if (err.response) {
      console.error('LEDGER RANGE STATUS >>>', err.response.status)
      console.error('LEDGER RANGE BODY   >>>', err.response.data)
    }
    throw err
  }
}

// Yeni gelir/gider satırı ekleyen fonksiyon
export async function createLedgerEntry(payload) {
  console.log('LEDGER PAYLOAD >>>', payload)
  try {
    const { data } = await http.post('/ledger', payload)
    console.log('LEDGER OK >>>', data)
    return data
  } catch (err) {
    console.error('LEDGER ERROR RAW >>>', err)
    if (err.response) {
      console.error('LEDGER ERROR STATUS >>>', err.response.status)
      console.error('LEDGER ERROR BODY   >>>', err.response.data)
    }
    throw err
  }
}
