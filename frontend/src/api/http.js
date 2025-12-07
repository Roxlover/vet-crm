import axios from 'axios'
import { getToken, clearAuth } from '../utils/auth'

const isBrowser = typeof window !== 'undefined'
const isNativeApp =
  isBrowser && (window.Capacitor || window.CapacitorRuntime)

// ==================================================
//  BASE URL ÇÖZÜCÜ
// ==================================================
function resolveBase() {
  const rawEnvBase = import.meta?.env?.VITE_API_BASE

  // 1) ENV VAR HER ŞEYDEN ÖNCE GELSİN (WEB + NATIVE)
  if (rawEnvBase && typeof rawEnvBase === 'string') {
    const cleaned = rawEnvBase.trim().replace(/\/$/, '')
    console.log('[HTTP][BASE][ENV]', cleaned)
    return cleaned
  }

  // 2) Native app’te asla window.location üzerinden localhost aramayalım
  if (isNativeApp) {
    const fallback = 'https://api.e-bullvet.com'
    console.log('[HTTP][BASE][NATIVE_FALLBACK]', fallback)
    return fallback
  }

  // 3) SSR / window yoksa
  if (!isBrowser) {
    console.log('[HTTP][BASE][NO_WINDOW] http://localhost:5239')
    return 'http://localhost:5239'
  }

  const { protocol, hostname } = window.location

  // 4) Local dev (web)
  if (hostname === 'localhost' || hostname === '127.0.0.1') {
    console.log('[HTTP][BASE][LOCALHOST] http://localhost:5239')
    return 'http://localhost:5239'
  }

  // 5) LAN IP
  const isLanIp =
    /^192\.168\./.test(hostname) ||
    /^10\./.test(hostname) ||
    /^172\.(1[6-9]|2\d|3[0-1])\./.test(hostname)

  if (isLanIp) {
    const lan = `http://${hostname}:5239`
    console.log('[HTTP][BASE][LAN]', lan)
    return lan
  }

  // 6) Production: origin
  const sameOrigin = `${protocol}//${hostname}`
  console.log('[HTTP][BASE][ORIGIN]', sameOrigin)
  return sameOrigin
}

const BASE = resolveBase()
console.log('[HTTP][BASE][FINAL]', BASE)

export const API_BASE = BASE

export const http = axios.create({
  baseURL: `${BASE}/api`,
  timeout: 15000,
})

// REQUEST INTERCEPTOR
http.interceptors.request.use((config) => {
  const token = getToken()
  const method = (config.method || 'GET').toUpperCase()
  const fullUrl = (config.baseURL || '') + (config.url || '')

  console.log('[HTTP][REQ]', method, config.url, {
    baseURL: config.baseURL,
    fullUrl,
    params: config.params,
    data: config.data,
  })

  if (token) {
    config.headers = config.headers || {}
    config.headers.Authorization = `Bearer ${token}`
  }

  return config
})

// RESPONSE INTERCEPTOR
http.interceptors.response.use(
  (res) => {
    const method = (res.config.method || 'GET').toUpperCase()
    console.log('[HTTP][RES]', method, res.config.url, {
      status: res.status,
      data: res.data,
    })
    return res
  },
  (err) => {
    if (err.response) {
      const status = err.response.status
      const method = (err.config?.method || 'GET').toUpperCase()
      const url = err.config?.url
      const fullUrl = (err.config?.baseURL || '') + (err.config?.url || '')

      console.log('[HTTP][ERR_RES]', method, url, {
        status,
        fullUrl,
        data: err.response.data,
      })

      if (status === 401) {
        console.warn('[AUTH] 401 geldi, token temizleniyor...')
        clearAuth()
        if (isBrowser && !isNativeApp) {
          window.location.href = '/login'
        }
      }
    } else if (err.request) {
      console.log('[HTTP][ERR_REQ_NO_RESPONSE]', err.message)
    } else {
      console.log('[HTTP][ERR_SETUP]', err.message)
    }

    return Promise.reject(err)
  }
)
