import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/',
    redirect: '/dashboard'
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: () => import('../views/Overview.vue')
  },
  {
    path: '/shipments',
    name: 'Shipments',
    component: () => import('../views/Shipments.vue')
  },
  {
    path: '/shipments/:id',
    name: 'ShipmentDetails',
    component: () => import('../views/ShipmentDetails.vue')
  },
  {
    path: '/new-order',
    name: 'NewOrder',
    component: () => import('../views/NewOrder.vue')
  }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

export default router
