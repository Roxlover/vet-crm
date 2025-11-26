// src/api/http.js
import axios from 'axios'
import { getToken, clearAuth } from '../utils/auth'

export const http = axios.create({
  baseURL: 'http://localhost:5239/api',
})

// REQUEST interceptor – header'a token ekle
http.interceptors.request.use((config) => {
  const token = getToken()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// RESPONSE interceptor – 401 gelirse login'e at
http.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response && err.response.status === 401) {
      console.warn('[AUTH] 401 geldi, token temizleniyor...')
      clearAuth()
      window.location.href = '/login'
    }
    return Promise.reject(err)
  },
)
