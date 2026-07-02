// src/router/index.js
import { createRouter, createWebHistory } from 'vue-router'
import { getUser, getToken } from '../utils/auth'
import DashboardView from '@/views/DashboardView.vue'
import OwnersView from '@/views/OwnersView.vue'
import PetsView from '@/views/PetsView.vue'
import VisitsView from '@/views/VisitsView.vue'
import BilancoView from '@/views/BilancoView.vue'
import DiseasesView from '@/views/DiseasesView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    // --- ORTAK GİRİŞ EKRANI (Herkese Açık) ---
    {
      path: '/',
      name: 'welcome',
      component: () => import('@/views/WelcomeView.vue'),
    },

    // --- VETERİNER / CRM ROTALARI ---
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/LoginView.vue'),
    },
    {
      path: '/dashboard',
      name: 'dashboard',
      component: DashboardView,
    },
    {
      path: '/owners',
      name: 'owners',
      component: OwnersView,
    },
    {
      path: '/bilanco',
      name: 'bilanco',
      component: BilancoView,
      meta: { requiresAuth: true },
    },
    {
      path: '/pets',
      name: 'pets',
      component: PetsView,
    },
    {
      path: '/diseases',
      name: 'diseases',
      component: DiseasesView,
    },
    {
      path: '/visits',
      name: 'visits',
      component: VisitsView,
    },
    {
      path: '/calendar',
      name: 'calendar',
      component: () => import('@/views/CalendarView.vue'),
    },

    // --- MÜŞTERİ PORTALI ROTALARI ---
    {
      path: '/client/login',
      name: 'client-login',
      component: () => import('@/views/client/ClientLoginView.vue'),
    },
    {
      path: '/client/dashboard',
      name: 'client-dashboard',
      component: () => import('@/views/client/ClientDashboardView.vue'),
    },
    {
      path: '/client/pets',
      name: 'client-pets',
      component: () => import('@/views/client/ClientPetsView.vue'),
    },
    {
      path: '/client/visits',
      name: 'client-visits',
      component: () => import('@/views/client/ClientVisitsView.vue'),
    },
  ],
})

router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('vetcrm_token')
  const userRaw = localStorage.getItem('vetcrm_user')
  const user = userRaw ? JSON.parse(userRaw) : null
  const isClient = user && (String(user.role).toLowerCase() === 'client')
  
  const isWelcome = to.path === '/'
  const isClientPath = to.path.startsWith('/client')
  const isClientLogin = to.path === '/client/login'
  const isVetLogin = to.path === '/login'

  console.log('[ROUTER GUARD]', { to: to.path, isClient, hasToken: !!token })

  // Welcome sayfası
  if (isWelcome) {
    if (token) {
      return next(isClient ? '/client/dashboard' : '/dashboard')
    }
    return next()
  }

  // MÜŞTERİ PORTALI ROTALARI
  if (isClientPath) {
    if (isClientLogin) {
      if (token && isClient) return next('/client/dashboard')
      return next()
    }
    
    // Korumalı müşteri sayfaları
    if (!token || !isClient) {
      return next('/client/login')
    }
    return next()
  }

  // VETERİNER / CRM ROTALARI
  if (isVetLogin) {
    if (token && !isClient) return next('/dashboard')
    return next()
  }

  // Korumalı veteriner sayfaları
  if (!token) {
    return next('/')
  }
  if (isClient) {
    return next('/client/dashboard')
  }

  // Bilanço yetkisi kontrolü
  if (to.path === '/bilanco') {
    const role = String(user?.role || '').trim().toLowerCase()
    const username = String(user?.username || '').trim().toLowerCase()
    const allowedUsers = ['bullboss']
    if (role !== 'admin' && !allowedUsers.includes(username)) {
      return next('/dashboard')
    }
  }

  return next()
})

export default router
