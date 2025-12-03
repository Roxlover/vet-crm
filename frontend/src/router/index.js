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

  // login yoksa -> login'e gönder
  if (authRequired && (!user || !token)) {
    return next('/login')
  }

  // login olmuşken /login'e gelmeye çalışma
  if (!authRequired && user && token) {
    return next('/')
  }

  // Bilanço sadece BullBoss
  if (to.path === '/bilanco' && (!user || user.username !== 'BullBoss')) {
    return next('/') 
  }

  // if (to.path === '/bilanco') {
  //   const username = user?.username?.toLowerCase().trim()
  //   const allowed = ['bullboss', 'sila']   // buraya istediğin isimleri ekleyebilirsin

  //   if (!username || !allowed.includes(username)) {
  //     return next('/')
  //   }
  // }
  next()
})

export default router
