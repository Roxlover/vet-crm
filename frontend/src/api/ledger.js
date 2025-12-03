import { http } from '@/api/http'

export async function fetchLedgerSummary(from, to, doctorId) {
  const params = { from, to }
  if (doctorId) params.doctorId = doctorId

  const { data } = await http.get('/ledger/summary', { params })
  return data
}


export async function fetchLedgerItems(from, to, doctorId) {
  const params = { from, to }
  if (doctorId) params.doctorId = doctorId

  const { data } = await http.get('/ledger/visit-items', { params })
  return data
}



export async function fetchLedgerRange(from, to, doctorId) {
  console.log('LEDGER RANGE PARAMS >>>', { from, to, doctorId })

  try {
    const params = { from, to }
    if (doctorId) params.doctorId = doctorId

    const { data } = await http.get('/ledger', {
      params,
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
export async function fetchLedgerByUser(from, to) {
  console.log('LEDGER BY USER PARAMS >>>', { from, to })

  const { data } = await http.get('/ledger/by-user', {
    params: { from, to },
  })

  console.log('LEDGER BY USER OK >>>', data)
  return data
}

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
