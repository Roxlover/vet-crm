// src/router/index.js
import { createRouter, createWebHistory } from 'vue-router'
import { getUser, getToken } from '../utils/auth'
import DashboardView from '@/views/DashboardView.vue'
import OwnersView from '@/views/OwnersView.vue'
import PetsView from '@/views/PetsView.vue'
import VisitsView from '@/views/VisitsView.vue'
import BilancoView from '@/views/BilancoView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/LoginView.vue'),
    },
    {
      path: '/',
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
      path: '/visits',
      name: 'visits',
      component: VisitsView,
    },
    {
      path: '/calendar',
      name: 'calendar',
      component: () => import('@/views/CalendarView.vue'),
    },
    // Müşteri Portalı Rotaları
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
  const isClientPath = to.path.startsWith('/client')
  const token = localStorage.getItem('vetcrm_token')
  const userRaw = localStorage.getItem('vetcrm_user')
  const user = userRaw ? JSON.parse(userRaw) : null
  const isClient = user && (String(user.role).toLowerCase() === 'client')

  console.log('[ROUTER]', {
    to: to.path,
    userRole: user?.role,
    isClient,
    hasToken: !!token,
  })

  if (isClientPath) {
    const isClientLogin = to.path === '/client/login'
    if (!isClientLogin && (!token || !isClient)) {
      return next('/client/login')
    }
    if (isClientLogin && token && isClient) {
      return next('/client/dashboard')
    }
    return next()
  } else {
    // Hekim / CRM sayfaları
    const isHekimLogin = to.path === '/login'
    if (!isHekimLogin && (!token || isClient)) {
      if (isClient) return next('/client/dashboard')
      return next('/login')
    }
    if (isHekimLogin && token && !isClient) {
      return next('/')
    }

    if (to.path === '/bilanco') {
      const role = String(user?.role || '').trim().toLowerCase()
      const username = String(user?.username || '').trim().toLowerCase()
      const allowedUsers = ['bullboss'] // whitelist
      const isAdmin = role === 'admin'
      const isExplicitAllowed = allowedUsers.includes(username)

      if (!isAdmin && !isExplicitAllowed) return next('/')
    }

    return next()
  }
})

export default router

