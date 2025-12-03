import axios from 'axios'
import { getToken, clearAuth } from '../utils/auth'

// ======================
//  BASE URL ÇÖZÜCÜ
//  - .env'de VITE_API_BASE varsa onu kullanır
//  - Yoksa window.location.hostname'e göre karar verir
//    • localhost → http://localhost:5239
//    • 192.168.x.x / 10.x.x.x / 172.16–31.x.x → o IP + :5239
//    • Diğer durumlar → aynı origin (https://panel.domain.com)
// ======================
function resolveBase() {
  // 1) .env üzerinden override imkânı (canlıya aldığımızda işimize çok yarar)
  // .env.local / .env.production içine:
  //   VITE_API_BASE=https://api.senin-domainin.com
  // yazarsan direkt burası geçerli olur.
  const envBase = import.meta?.env?.VITE_API_BASE
  if (envBase) {
    return envBase.replace(/\/$/, '') // sonda / varsa kırp
  }

  // 2) window yoksa (test vs.) fallback
  if (typeof window === 'undefined') {
    return 'http://localhost:5239'
  }

  const { protocol, hostname } = window.location

  // 3) PC'de local dev (sadece kendi tarayıcından bakarken)
  if (hostname === 'localhost' || hostname === '127.0.0.1') {
    return 'http://localhost:5239'
  }

  // 4) Aynı Wi-Fi'den telefon / başka PC ile bağlanma (LAN IP)
  const isLanIp =
    /^192\.168\./.test(hostname) ||
    /^10\./.test(hostname) ||
    /^172\.(1[6-9]|2\d|3[0-1])\./.test(hostname)

  if (isLanIp) {
    // Örn: telefon tarayıcısında 192.168.1.37:5173 açarsan
    // API_BASE → http://192.168.1.37:5239 olur
    return `http://${hostname}:5239`
  }

  // 5) Production: API ve panel aynı origin'de ise
  // Örn: https://panel.domain.com/api
  return `${protocol}//${hostname}`
}

const BASE = resolveBase()

export const API_BASE = BASE

export const http = axios.create({
  baseURL: `${BASE}/api`,
  timeout: 10000,
})

http.interceptors.request.use((config) => {
  const token = getToken()

  console.log(
    '[HTTP][REQ]',
    config.method?.toUpperCase(),
    config.url,
    {
      baseURL: config.baseURL,
      data: config.data,
    },
  )

  if (token) {
    config.headers = config.headers || {}
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

http.interceptors.response.use(
  (res) => {
    console.log(
      '[HTTP][RES]',
      res.config.method?.toUpperCase(),
      res.config.url,
      {
        status: res.status,
        data: res.data,
      },
    )
    return res
  },
  (err) => {
    if (err.response) {
      console.log(
        '[HTTP][ERR_RES]',
        err.config?.method?.toUpperCase(),
        err.config?.url,
        {
          status: err.response.status,
          data: err.response.data,
        },
      )
    } else if (err.request) {
      console.log('[HTTP][ERR_REQ_NO_RESPONSE]', err.message)
    } else {
      console.log('[HTTP][ERR_SETUP]', err.message)
    }

    if (err.response && err.response.status === 401) {
      console.warn('[AUTH] 401 geldi, token temizleniyor...')
      clearAuth()
      window.location.href = '/login'
    }
    return Promise.reject(err)
  },
)
