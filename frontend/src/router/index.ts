import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'order',
      component: () => import('../views/OrderView.vue'),
    },
    {
      path: '/odeme/:orderNo',
      name: 'checkout',
      component: () => import('../views/CheckoutView.vue'),
    },
    {
      path: '/result/:paymentNo',
      name: 'result',
      component: () => import('../views/ResultView.vue'),
    },
  ],
})

export default router