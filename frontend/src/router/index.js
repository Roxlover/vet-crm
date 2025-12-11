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
      name: 'Bilanco',
      component: BilancoView,
      meta: { requiresAuth: true, roles: ['Admin'] },
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

  if (authRequired && (!user || !token)) {
    return next('/login')
  }

  if (!authRequired && user && token) {
    return next('/')
  }

  if (to.path === '/bilanco') {
    const role = user?.role?.trim()
    const username = user?.username?.toLowerCase().trim()
    const allowedUsers = ['bullboss']  

    const isAdmin = role === 'Admin'
    const isExplicitAllowed = username && allowedUsers.includes(username)

    if (!isAdmin && !isExplicitAllowed) {
      return next('/')
    }
  }

  next()
})


export default router
