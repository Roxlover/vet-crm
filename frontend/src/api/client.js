import axios from 'axios'

const apiClient = axios.create({
  // baseURL: 'http://localhost:5239/api',
  baseURL: 'http://192.168.1.134:5239/api',
  timeout: 10000,
})

export default apiClient
