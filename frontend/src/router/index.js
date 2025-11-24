import { createRouter, createWebHistory } from 'vue-router'

import DashboardView from '../views/DashboardView.vue'
import OwnersView from '../views/OwnersView.vue'
import PetsView from '../views/PetsView.vue'
import VisitsView from '../views/VisitsView.vue'
import BilancoView from '@/views/BilancoView.vue'

const routes = [
{
  path: '/',
  name: 'dashboard',
  component: () => import('../views/DashboardView.vue'),
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
  component: () => import('../views/CalendarView.vue'),
}
]

const router = createRouter({
  history: createWebHistory(),
  routes,

})

export default router
