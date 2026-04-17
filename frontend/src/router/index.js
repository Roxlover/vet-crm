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
  ],
})

router.beforeEach((to, from, next) => {
  const publicPages = ['/login']
  const authRequired = !publicPages.includes(to.path)

  const token = localStorage.getItem('vetcrm_token')
  const userRaw = localStorage.getItem('vetcrm_user')
  const user = userRaw ? JSON.parse(userRaw) : null

  console.log('[ROUTER]', {
    to: to.path,
    metaRoles: to.meta?.roles,
    userRole: user?.role,
    userUsername: user?.username,
    hasToken: !!token,
  })
  if (to.path === '/bilanco') {
  const role = String(user?.role || '').trim()                 // Admin
  const username = String(user?.username || '').trim().toLowerCase()

  const allowedUsers = ['bullboss'] // whitelist
  const isAdmin = role === 'Admin'
  const isExplicitAllowed = allowedUsers.includes(username)

  if (!isAdmin && !isExplicitAllowed) return next('/')
}

  // 1) Auth gerekiyorsa ve yoksa -> login
  if (authRequired && (!user || !token)) {
    return next('/login')
  }

  // 2) Login sayfasına auth’lu girilirse -> dashboard
  if (!authRequired && user && token) {
    return next('/')
  }

  // 3) Role kontrolü (varsa)
  const requiredRoles = to.meta?.roles
  if (requiredRoles && requiredRoles.length) {
    const role = String(user?.role || '').trim().toLowerCase()
    const username = String(user?.username || '').trim().toLowerCase()
    const allowed = requiredRoles.map(r => String(r).trim().toLowerCase())

    const ok = allowed.includes(role) || username === 'bullboss'
    if (!ok) {
      return next('/') // yetkisiz -> dashboard
    }
  }

  // 4) HER HALÜKARDA devam et
  return next()
})


export default router
